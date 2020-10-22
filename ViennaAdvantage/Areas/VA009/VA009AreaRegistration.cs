using System.Web.Mvc;
using System.Web.Optimization;



//NOTE:--    Please replace ViennaAdvantage with prefix of your module..



namespace ViennaAdvantage //  Please replace namespace with prefix of your module..
{
    public class ViennaAdvantageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VA009";   //Please replace "ViennaAdvantage" with prefix of your module.......
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "VA009_default",
                "VA009/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                , new[] { "VA009.Controllers" }
            );    // Please replace ViennaAdvantage with prefix of your module...


            StyleBundle style = new StyleBundle("~/Areas/VA009/Contents/VA009Style");

            /* ==>  Here include all css files in style bundle......see example below....  */
            ScriptBundle script = new ScriptBundle("~/Areas/VA009/Scripts/VA009Js");

            style.Include("~/Areas/VA009/Contents/VA009_PaymentFrm.css");

            style.Include("~/Areas/VA009/Contents/VA009_rtl.css");


            script.Include("~/Areas/VA009/Scripts/apps/forms/PaymentForm.js",
                "~/Areas/VA009/Scripts/model/callouts.js",
            "~/Areas/VA009/Scripts/apps/forms/DownloadXML.js",
            "~/Areas/VA009/Scripts/apps/forms/DownloadDATFile.js");

            //script.Include("~/Areas/VA009/Scripts/VA009.all.min.js");
            //style.Include("~/Areas/VA009/Contents/VA009.all.min.css");



            /*-------------------------------------------------------
                    Here include all js files in style bundle......see example below....
             --------------------------------------------------------*/


            //script.Include("~/Areas/ViennaAdvantage/Scripts/example1.js",
            //               "~/Areas/ViennaAdvantage/Scripts/example2.js");




            /*-------------------------------------------------------
              Please replace "ViennaAdvantage" with prefix of your module..
             * 
             * 1. first parameter is script/style bundle...
             * 
             * 2. Second parameter is module prefix...
             * 
             * 3. Third parameter is order of loading... (dafault is 10 )
             * 
             --------------------------------------------------------*/

            VAdvantage.ModuleBundles.RegisterScriptBundle(script, "VA009", 10);
            VAdvantage.ModuleBundles.RegisterStyleBundle(style, "VA009", 10);
        }
    }
}