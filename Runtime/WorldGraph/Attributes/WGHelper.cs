using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThunderNut.SceneManagement {

    public static class WGHelper {
        public static IEnumerable<FieldInfo> GetFieldInfosWithAttribute(object obj, Type attribute) {
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            IEnumerable<FieldInfo> fieldsWithAttribute = fields.Where(x => x.IsDefined(attribute, false));
            return fieldsWithAttribute;
        }
    }

}