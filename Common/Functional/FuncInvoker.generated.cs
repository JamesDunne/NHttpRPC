using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, TResult>
    {
        private readonly Func<T1, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1)
        {
            TResult val;
            val = _func(t1);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <returns></returns>
        public TResult this[T1 t1]
        {
            get
            {
                return Invoke(t1);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, TResult>
    {
        private readonly Func<T1, T2, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2)
        {
            TResult val;
            val = _func(t1, t2);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2]
        {
            get
            {
                return Invoke(t1, t2);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, TResult>
    {
        private readonly Func<T1, T2, T3, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3)
        {
            TResult val;
            val = _func(t1, t2, t3);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3]
        {
            get
            {
                return Invoke(t1, t2, t3);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, T4, TResult>
    {
        private readonly Func<T1, T2, T3, T4, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, T4, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            TResult val;
            val = _func(t1, t2, t3, t4);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3, T4 t4]
        {
            get
            {
                return Invoke(t1, t2, t3, t4);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, T4, T5, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            TResult val;
            val = _func(t1, t2, t3, t4, t5);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3, T4 t4, T5 t5]
        {
            get
            {
                return Invoke(t1, t2, t3, t4, t5);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, T4, T5, T6, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            TResult val;
            val = _func(t1, t2, t3, t4, t5, t6);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6]
        {
            get
            {
                return Invoke(t1, t2, t3, t4, t5, t6);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, T4, T5, T6, T7, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <param name="t7"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            TResult val;
            val = _func(t1, t2, t3, t4, t5, t6, t7);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <param name="t7"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7]
        {
            get
            {
                return Invoke(t1, t2, t3, t4, t5, t6, t7);
            }
        }
    }

    /// <summary>
    /// Invokes the function with the set of parameter values in various ways.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public struct FuncInvoker<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _func;

        /// <summary>
        /// Creates a function invoker to invoke functions with parameters in various ways.
        /// </summary>
        public FuncInvoker(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this._func = func;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <param name="t7"></param>
        /// <param name="t8"></param>
        /// <returns></returns>
        public TResult Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            TResult val;
            val = _func(t1, t2, t3, t4, t5, t6, t7, t8);
            return val;
        }

        /// <summary>
        /// Invokes the function with the parameter values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <param name="t7"></param>
        /// <param name="t8"></param>
        /// <returns></returns>
        public TResult this[T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8]
        {
            get
            {
                return Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
            }
        }
    }

    public static partial class Function
    {
		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, TResult> MakeInvoker<T1, TResult>(Func<T1, TResult> func)
        {
            return new FuncInvoker<T1, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, TResult> MakeInvoker<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return new FuncInvoker<T1, T2, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, TResult> MakeInvoker<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, T4, TResult> MakeInvoker<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, T4, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, T4, T5, TResult> MakeInvoker<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, T4, T5, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, T4, T5, T6, TResult> MakeInvoker<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, T4, T5, T6, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, T4, T5, T6, T7, TResult> MakeInvoker<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, T4, T5, T6, T7, TResult>(func);
        }

		/// <summary>
		/// Makes a FuncInvoker out of the given Func.
		/// </summary>
        /// <param name="func"></param>
        public static FuncInvoker<T1, T2, T3, T4, T5, T6, T7, T8, TResult> MakeInvoker<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
        {
            return new FuncInvoker<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(func);
        }

    }
}