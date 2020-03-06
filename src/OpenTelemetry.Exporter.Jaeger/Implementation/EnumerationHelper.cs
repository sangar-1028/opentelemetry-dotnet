﻿// <auto-generated>
// <copyright file="EnumerationHelper.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenTelemetry.Exporter.Jaeger.Implementation
{
    internal class DictionaryEnumerator<TKey, TValue, TState> : Enumerator
        <IEnumerable<KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>,
        TState>
        where TState : struct
    {
        protected DictionaryEnumerator()
        {
        }
    }

    internal class ListEnumerator<TValue, TState> : Enumerator
        <IEnumerable<TValue>,
        TValue,
        TState>
        where TState : struct
    {
        protected ListEnumerator()
        {
        }
    }

    // A helper class for enumerating over IEnumerable<TItem> without allocation if a struct enumerator is available.
    internal class Enumerator<TEnumerable, TItem, TState>
        where TEnumerable : IEnumerable<TItem>
        where TState : struct
    {
        private static readonly MethodInfo GenericGetEnumeratorMethod = typeof(IEnumerable<TItem>).GetMethod("GetEnumerator");
        private static readonly MethodInfo GeneircCurrentGetMethod = typeof(IEnumerator<TItem>).GetProperty("Current").GetMethod;
        private static readonly MethodInfo MoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod("Dispose");
        private static readonly ConcurrentDictionary<Type, AllocationFreeForEachDelegate> AllocationFreeForEachDelegates = new ConcurrentDictionary<Type, AllocationFreeForEachDelegate>();
        private static readonly Func<Type, AllocationFreeForEachDelegate> BuildAllocationFreeForEachDelegateRef = BuildAllocationFreeForEachDelegate;

        private delegate void AllocationFreeForEachDelegate(TEnumerable instance, ref TState state, ForEachDelegate itemCallback);

        public delegate bool ForEachDelegate(ref TState state, TItem item);

        protected Enumerator()
        {
        }

        public static void AllocationFreeForEach(TEnumerable instance, ref TState state, ForEachDelegate itemCallback)
        {
            Debug.Assert(instance != null && itemCallback != null);

            var type = instance.GetType();

            var allocationFreeForEachDelegate = AllocationFreeForEachDelegates.GetOrAdd(
                type,
                BuildAllocationFreeForEachDelegateRef);

            allocationFreeForEachDelegate(instance, ref state, itemCallback);
        }

        /* We want to do this type of logic...
            public static void AllocationFreeForEach(Dictionary<string, int> dictionary, ref TState state, ForEachDelegate itemCallback)
            {
                using (Dictionary<string, int>.Enumerator enumerator = dictionary.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!itemCallback(ref state, enumerator.Current))
                            break;
                    }
                }
            }
            ...because it takes advantage of the struct Enumerator on the built-in types which give an allocation-free way to enumerate.
        */
        private static AllocationFreeForEachDelegate BuildAllocationFreeForEachDelegate(Type enumerableType)
        {
            var itemCallbackType = typeof(ForEachDelegate);

            var getEnumeratorMethod = ResolveGetEnumeratorMethodForType(enumerableType);
            if (getEnumeratorMethod == null)
            {
                // Fallback to allocation mode and use IEnumerable<TItem>.GetEnumerator.
                // Primarily for Array.Empty and Enumerable.Empty case, but also for user types.
                getEnumeratorMethod = GenericGetEnumeratorMethod;
            }

            var enumeratorType = getEnumeratorMethod.ReturnType;

            var dynamicMethod = new DynamicMethod(
                nameof(AllocationFreeForEach),
                null,
                new[] { typeof(TEnumerable), typeof(TState).MakeByRefType(), itemCallbackType },
                typeof(AllocationFreeForEachDelegate).Module,
                skipVisibility: true);

            var generator = dynamicMethod.GetILGenerator();

            generator.DeclareLocal(enumeratorType);

            var beginLoopLabel = generator.DefineLabel();
            var processCurrentLabel = generator.DefineLabel();
            var returnLabel = generator.DefineLabel();
            var breakLoopLabel = generator.DefineLabel();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, getEnumeratorMethod);
            generator.Emit(OpCodes.Stloc_0);

            // try
            generator.BeginExceptionBlock();
            {
                generator.Emit(OpCodes.Br_S, beginLoopLabel);

                generator.MarkLabel(processCurrentLabel);

                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloca_S, 0);
                generator.Emit(OpCodes.Constrained, enumeratorType);
                generator.Emit(OpCodes.Callvirt, GeneircCurrentGetMethod);

                generator.Emit(OpCodes.Callvirt, itemCallbackType.GetMethod("Invoke"));

                generator.Emit(OpCodes.Brtrue_S, beginLoopLabel);

                generator.Emit(OpCodes.Leave_S, returnLabel);

                generator.MarkLabel(beginLoopLabel);

                generator.Emit(OpCodes.Ldloca_S, 0);
                generator.Emit(OpCodes.Constrained, enumeratorType);
                generator.Emit(OpCodes.Callvirt, MoveNextMethod);

                generator.Emit(OpCodes.Brtrue_S, processCurrentLabel);

                generator.MarkLabel(breakLoopLabel);

                generator.Emit(OpCodes.Leave_S, returnLabel);
            }

            // finally
            generator.BeginFinallyBlock();
            {
                if (typeof(IDisposable).IsAssignableFrom(enumeratorType))
                {
                    generator.Emit(OpCodes.Ldloca_S, 0);
                    generator.Emit(OpCodes.Constrained, enumeratorType);
                    generator.Emit(OpCodes.Callvirt, DisposeMethod);
                }
            }

            generator.EndExceptionBlock();

            generator.MarkLabel(returnLabel);

            generator.Emit(OpCodes.Ret);

            return (AllocationFreeForEachDelegate)dynamicMethod.CreateDelegate(typeof(AllocationFreeForEachDelegate));
        }

        private static MethodInfo ResolveGetEnumeratorMethodForType(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.Name == "GetEnumerator" && !method.ReturnType.IsInterface)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
