using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Mvc.Bootstrap.Datatables;

[assembly: PreApplicationStartMethod(typeof(Mvc.Bootstrap.Datatables.Example.App_Start.RegisterDatatablesModelBinder), "Start")]

namespace Mvc.Bootstrap.Datatables.Example.App_Start
{
    public static class RegisterDatatablesModelBinder {
        public static void Start() {
            // Check if a binder for this type hasn't been added yet
            if (!ModelBinders.Binders.ContainsKey(typeof(DataTablesParam)))
            {
                ModelBinders.Binders.Add(typeof(DataTablesParam), new NullableDataTablesModelBinder());
            }
        }
    }
}
