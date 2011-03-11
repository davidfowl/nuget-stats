using System;

public class Lazy {    
    public static void Run(Action action) {
        Run<object>(() => {
            action();
            return null;
        });
    }

    public static T Run<T>(Func<T> factory) {
        return new Lazy<T>(factory).Value;
    }
}