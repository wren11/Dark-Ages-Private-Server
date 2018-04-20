using System;

namespace Darkages.Common
{
    public class Predicate
    {
        public static Predicate<T> Or<T>(params Predicate<T>[] predicates)
        {
            return delegate (T item)
            {
                foreach (var predicate in predicates)
                    if (predicate(item))
                        return true;
                return false;
            };
        }

        public static Predicate<T> And<T>(params Predicate<T>[] predicates)
        {
            return delegate (T item)
            {
                foreach (var predicate in predicates)
                    if (!predicate(item))
                        return false;
                return true;
            };
        }
    }
}