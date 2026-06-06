using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace InterfazHubSpot
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // -------------------------------------------------
            //   Estilos generales
            // -------------------------------------------------

            var stylesSegoe = new StyleBundle("~/Content/Segoe/css")
                .Include("~/Content/Segoe.css", new CssRewriteUrlTransform());

            stylesSegoe.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesSegoe.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesSegoe);

            var stylesLogin = new StyleBundle("~/Content/Login/css")
                .Include("~/Content/Login2.css", new CssRewriteUrlTransform());

            stylesLogin.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesLogin.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesLogin);

            var stylesHome = new StyleBundle("~/Content/Home/css")
                .Include("~/Content/Home.css", new CssRewriteUrlTransform());

            stylesHome.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesHome.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesHome);

            var stylesCloudflare = new StyleBundle("~/Content/cloudflare/css")
                    .Include("~/Content/jquery.mCustomScrollbar.min.css", new CssRewriteUrlTransform());

            stylesCloudflare.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesCloudflare.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesCloudflare);

            var stylesAwesome = new StyleBundle("~/Content/awesome/css")
                .Include("~/Content/awesome/all.css", new CssRewriteUrlTransform());

            stylesAwesome.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesAwesome.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesAwesome);

            var stylesBootstrap = new StyleBundle("~/Content/bootstrap/css")
                   .Include("~/Content/bootstrap.min.css", new CssRewriteUrlTransform())
                   .Include("~/Content/bootstrap-switch.min.css", new CssRewriteUrlTransform())
                   .Include("~/Content/bootstrap-dialog.min.css", new CssRewriteUrlTransform())
                   .Include("~/Content/fs-modal.min.css", new CssRewriteUrlTransform());

            stylesBootstrap.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesBootstrap.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesBootstrap);

            // -------------------------------------------------
            //   Kendo Themes
            // -------------------------------------------------

            var stylesKendoBlack = new StyleBundle("~/Content/cssblack")
                            .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.black.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.black.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoBlack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoBlack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoBlack);

            var stylesKendoMsBlack = new StyleBundle("~/Content/cssmsblack")
                .Include("~/Content/mastersoft.black.css", new CssRewriteUrlTransform());

            stylesKendoMsBlack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsBlack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsBlack);

            var stylesKendoBlueopal = new StyleBundle("~/Content/cssblueopal")
                            .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.blueopal.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.blueopal.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoBlueopal.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoBlueopal.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoBlueopal);

            var stylesKendoBootstrap = new StyleBundle("~/Content/cssbootstrap")
                            .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.bootstrap.min.css", new CssRewriteUrlTransform())
                            .Include("~/Content/kendo/kendo.bootstrap.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoBootstrap.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoBootstrap.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoBootstrap);

            var stylesKendoDefault = new StyleBundle("~/Content/cssdefault")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.default.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.default.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoDefault.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoDefault.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoDefault);

            var stylesKendoFiori = new StyleBundle("~/Content/cssfiori")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.fiori.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.fiori.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoFiori.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoFiori.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoFiori);

            var stylesKendoFlat = new StyleBundle("~/Content/cssflat")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.flat.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.flat.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoFlat.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoFlat.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoFlat);

            var stylesKendoMsFlat = new StyleBundle("~/Content/cssmsflat")
                .Include("~/Content/mastersoft.flat.css", new CssRewriteUrlTransform());

            stylesKendoMsFlat.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsFlat.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsFlat);

            var stylesKendoHighcontrast = new StyleBundle("~/Content/csshighcontrast")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.highcontrast.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.highcontrast.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoHighcontrast.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoHighcontrast.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoHighcontrast);

            var stylesKendoMsHighcontrast = new StyleBundle("~/Content/cssmshighcontrast")
                .Include("~/Content/mastersoft.highcontrast.css", new CssRewriteUrlTransform());

            stylesKendoMsHighcontrast.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsHighcontrast.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsHighcontrast);

            var stylesKendoMaterial = new StyleBundle("~/Content/cssmaterial")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.material.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.material.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoMaterial.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMaterial.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMaterial);

            var stylesKendoMsMaterial = new StyleBundle("~/Content/cssmsmaterial")
                .Include("~/Content/mastersoft.material.css", new CssRewriteUrlTransform());

            stylesKendoMsMaterial.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsMaterial.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsMaterial);

            var stylesKendoMaterialblack = new StyleBundle("~/Content/cssmaterialblack")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.materialblack.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.materialblack.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoMaterialblack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMaterialblack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMaterialblack);

            var stylesKendoMsMaterialBlack = new StyleBundle("~/Content/cssmsmaterialblack")
                .Include("~/Content/mastersoft.materialblack.css", new CssRewriteUrlTransform());

            stylesKendoMsMaterialBlack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsMaterialBlack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsMaterialBlack);

            var stylesKendoMetro = new StyleBundle("~/Content/cssmetro")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.metro.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.metro.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoMetro.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMetro.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMetro);

            var stylesKendoMsMetro = new StyleBundle("~/Content/cssmsmetro")
                .Include("~/Content/mastersoft.material.css", new CssRewriteUrlTransform());

            stylesKendoMsMetro.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsMetro.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsMetro);

            var stylesKendoMetroblack = new StyleBundle("~/Content/cssmetroblack")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.metroblack.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.metroblack.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoMetroblack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMetroblack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMetroblack);

            var stylesKendoMsMetroBlack = new StyleBundle("~/Content/cssmsmetroblack")
                .Include("~/Content/mastersoft.metroblack.css", new CssRewriteUrlTransform());

            stylesKendoMsMetroBlack.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsMetroBlack.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsMetroBlack);

            var stylesKendoMoonlight = new StyleBundle("~/Content/cssmoonlight")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.moonlight.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.moonlight.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoMoonlight.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMoonlight.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMoonlight);

            var stylesKendoMsMoonlight = new StyleBundle("~/Content/cssmsmoonlight")
                .Include("~/Content/mastersoft.moonlight.css", new CssRewriteUrlTransform());

            stylesKendoMsMoonlight.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsMoonlight.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsMoonlight);

            var stylesKendoNova = new StyleBundle("~/Content/cssnova")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.nova.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.nova.mobile.min.css", new CssRewriteUrlTransform());

            stylesKendoNova.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoNova.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoNova);

            var stylesOffice365 = new StyleBundle("~/Content/cssoffice365")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.office365.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.office365.mobile.min.css", new CssRewriteUrlTransform());

            stylesOffice365.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesOffice365.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesOffice365);

            var stylesKendoMsOffice365 = new StyleBundle("~/Content/cssmsoffice365")
                .Include("~/Content/mastersoft.office365.css", new CssRewriteUrlTransform());

            stylesKendoMsOffice365.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesKendoMsOffice365.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesKendoMsOffice365);

            var stylesSilver = new StyleBundle("~/Content/csssilver")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.silver.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.silver.mobile.min.css", new CssRewriteUrlTransform());

            stylesSilver.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesSilver.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesSilver);

            var stylesUniform = new StyleBundle("~/Content/cssuniform")
                .Include("~/Content/kendo/kendo.common.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.rtl.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.uniform.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/kendo/kendo.uniform.mobile.min.css", new CssRewriteUrlTransform());

            stylesUniform.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesUniform.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesUniform);

            // -------------------------------------------------
            //   Estilos de Mastersoft
            // -------------------------------------------------

            var stylesMenustyle = new StyleBundle("~/Content/menustyle/css")
                .Include("~/Content/menustyle2.css", new CssRewriteUrlTransform());

            stylesMenustyle.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesMenustyle.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesMenustyle);

            var stylesMastersoft = new StyleBundle("~/Content/mastersoft/css")
                .Include("~/Content/Mastersoft.css", new CssRewriteUrlTransform());

            stylesMastersoft.Transforms.Insert(0, new StyleRelativePathTransform());
            stylesMastersoft.Orderer = new AsIsBundleOrderer();

            bundles.Add(stylesMastersoft);

            // -------------------------------------------------
            //   Scripts generales
            // -------------------------------------------------

            var scriptsJquery = new ScriptBundle("~/bundles/jquery").Include(
                                        "~/Scripts/kendo/jquery.min.js",
                                        "~/Scripts/jquery-ui.js");

            scriptsJquery.Orderer = new AsIsBundleOrderer();

            bundles.Add(scriptsJquery);

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // preparado para la producción y podrá utilizar la herramienta de compilación disponible en http://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/cloudflare").Include(
                        "~/Scripts/jquery.mCustomScrollbar.concat.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/bootstrap-dialog.min.js",
                        "~/Scripts/jquery.bootstrap.wizard.min.js",
                        "~/Scripts/fs-modal.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                       "~/Content/assets/global/plugins/js.cookie.min.js",
                       "~/Content/assets/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js",
                       "~/Content/assets/global/plugins/jquery.blockui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                        "~/Scripts/kendo/jszip.min.js",
                        "~/Scripts/kendo/kendo.all.min.js",
                        // "~/Scripts/kendo/kendo.timezones.min.js", // uncomment if using the Scheduler
                        "~/Scripts/kendo/cultures/kendo.culture.es-AR.min.js",
                        "~/Scripts/kendo/messages/kendo.messages.es-AR.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                        "~/Scripts/Login2.js"));

            bundles.Add(new ScriptBundle("~/bundles/presentacion").Include(
                        "~/Scripts/Presentacion.js"));

            bundles.Add(new ScriptBundle("~/bundles/home").Include(
                        "~/Scripts/Home.js"));

            bundles.Add(new ScriptBundle("~/bundles/menu").Include(
                        "~/Scripts/Menu2.js"));

            bundles.Add(new ScriptBundle("~/bundles/mastersoft").Include(
                    "~/Scripts/Mastersoft.js"));

            bundles.Add(new ScriptBundle("~/bundles/PrcOlvidoPassword").Include(
                    "~/Scripts/App/PrcOlvidoPassword.js"));

            bundles.Add(new ScriptBundle("~/bundles/PrcApariencia").Include(
                        "~/Scripts/Apariencia.js"));

            // -------------------------------------------------
            //   Scripts de aplicacion
            // -------------------------------------------------

            bundles.Add(new ScriptBundle("~/bundles/AbmEmpresas").Include(
                        "~/Scripts/App/AbmEmpresas.js"));

            bundles.Add(new ScriptBundle("~/bundles/AbmUsuariosWeb").Include(
                        "~/Scripts/App/AbmUsuariosWeb.js"));

            //------------------------

            bundles.IgnoreList.Clear();
        }
    }


    public class StyleRelativePathTransform : IBundleTransform
    {
        private static Regex pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);

        public void Process(BundleContext context, BundleResponse response)
        {
            response.Content = string.Empty;

            foreach (BundleFile file in response.Files)
            {
                using (var reader = new StreamReader(file.VirtualFile.Open()))
                {
                    var contents = reader.ReadToEnd();
                    var matches = pattern.Matches(contents);

                    if (matches.Count > 0)
                    {
                        var directoryPath = VirtualPathUtility.GetDirectory(file.VirtualFile.VirtualPath);

                        foreach (Match match in matches)
                        {
                            var fileRelativePath = match.Groups[2].Value;
                            var fileVirtualPath = VirtualPathUtility.Combine(directoryPath, fileRelativePath);
                            var quote = match.Groups[1].Value;
                            var replace = String.Format("url({0}{1}{0})", quote, VirtualPathUtility.ToAbsolute(fileVirtualPath));

                            contents = contents.Replace(match.Groups[0].Value, replace);
                        }

                    }

                    response.Content = String.Format("{0}\r\n{1}", response.Content, contents);
                }
            }
        }
    }


    public class AsIsBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<FileInfo> OrderFiles(BundleContext context, IEnumerable<FileInfo> files)
        {
            return files;
        }

        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }


}



































































