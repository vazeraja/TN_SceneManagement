using System;

namespace ThunderNut.SceneManagement {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public abstract class ContextFilterableAttribute : Attribute { }

}