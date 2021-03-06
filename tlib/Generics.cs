﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using TLib.Interfaces;

namespace TLib
{
    public class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }

        public void NotifyAll(T p)
        {
            foreach (IObserver<T> o in _observers)
            {
                o.OnNext(p);
            }
        }
    }

    public class Range<T>
    {
        private T min;
        private T val;
        private T max;

        public Range(T mn, T v, T mx) { val = v;  min = mn; max = mx; }

        public T Value
        {
            get { return val; }
            set { val = value; }
        }

        public T Min
        {
            get { return min; }
            set { max = value; }
        }
        public T Max
        {
            get { return max; }
            set { max = value; }
        }
    }

    public delegate object GenericInvoker(object target, params object[] arguments);

    public static class DynamicMethods
    {
        #region Constants
        #endregion // Constants

        #region Fields
        #endregion // Fields

        #region Constructors
        #endregion // Constructors

        #region Properties
        #endregion // Properties

        #region Operators
        #endregion // Operators

        #region Events
        #endregion // Events

        #region Methods
        private static void FindMethod(Type type, string methodName, Type[] typeArguments, Type[] parameterTypes, out MethodInfo methodInfo,
          out ParameterInfo[] parameters)
        {

            methodInfo = null;
            parameters = null;

            if (null == parameterTypes)
            {
                methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                methodInfo = methodInfo.MakeGenericMethod(typeArguments);
                parameters = methodInfo.GetParameters();
            }
            else
            {
                // Method is probably overloaded. As far as i know there's no other way to get the MethodInfo instance, we have to
                // search for it in all the type methods
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodInfo method in methods)
                {
                    if (method.Name == methodName)
                    {
                        // create the generic method
                        MethodInfo genericMethod = method.MakeGenericMethod(typeArguments);
                        parameters = genericMethod.GetParameters();

                        // compare the method parameters
                        if (parameters.Length == parameterTypes.Length)
                        {
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].ParameterType != parameterTypes[i])
                                {
                                    continue; // this is not the method we'r looking for
                                }
                            }

                            // if we'r here, we got the rigth method
                            methodInfo = genericMethod;
                            break;
                        }
                    }
                }

                if (null == methodInfo)
                {
                    throw new InvalidOperationException("Method not found");
                }
            }
        }

        public static GenericInvoker GenericMethodInvokerMethod(Type type, string methodName, Type[] typeArguments, Type[] parameterTypes)
        {
            MethodInfo methodInfo;
            ParameterInfo[] parameters;

            // find the method to be invoked
            FindMethod(type, methodName, typeArguments, parameterTypes, out methodInfo, out parameters);

            string name = string.Format("__MethodInvoker_{0}_ON_{1}", methodInfo.Name, methodInfo.DeclaringType.Name);
            DynamicMethod dynamicMethod = new DynamicMethod(name, typeof(object), new Type[] { typeof(object), typeof(object[]) },
              methodInfo.DeclaringType);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            // define local vars
            generator.DeclareLocal(typeof(object));

            // load first argument, the instace where the method is to be invoked
            generator.Emit(OpCodes.Ldarg_0);

            // cast to the correct type
            generator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);

            for (int i = 0; i < parameters.Length; i++)
            {
                // load paramters they are passed as an object array
                generator.Emit(OpCodes.Ldarg_1);

                // load array element
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldelem_Ref);

                // cast or unbox parameter as needed
                Type parameterType = parameters[i].ParameterType;
                if (parameterType.IsClass)
                {
                    generator.Emit(OpCodes.Castclass, parameterType);
                }
                else
                {
                    generator.Emit(OpCodes.Unbox_Any, parameterType);
                }
            }

            // call method
            generator.EmitCall(OpCodes.Callvirt, methodInfo, null);

            // handle method return if needed
            if (methodInfo.ReturnType == typeof(void))
            {
                // return null
                generator.Emit(OpCodes.Ldnull);
            }
            else
            {
                // box value if needed
                if (methodInfo.ReturnType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, methodInfo.ReturnType);
                }
            }

            // store to the local var
            generator.Emit(OpCodes.Stloc_0);

            // load local and return
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);

            // return delegate
            return (GenericInvoker)dynamicMethod.CreateDelegate(typeof(GenericInvoker));
        }

        public static GenericInvoker GenericMethodInvokerMethod(Type type, string methodName, Type[] typeArguments)
        {
            return GenericMethodInvokerMethod(type, methodName, typeArguments, null);
        }
        #endregion // Methods
    }
}
