namespace Dragonfly.UmbracoTheming
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using ClientDependency.Core.Mvc;
    using Umbraco.Core;
    using Umbraco.Core.IO;
    using Umbraco.Core.Logging;

    public static class ThemeHelper
    {
        public enum PathType
        {
            ThemeRoot,
            View,
            PartialView,
            GridEditor
        };

        /// <summary>
        /// Returns the final path to the requested type, based on the theme and file existence.
        /// </summary>
        /// <param name="SiteThemeName">Theme Name</param>
        /// <param name="PathType">Type of Path to return</param>
        /// <param name="ViewName">Name of the View (without extension) (optional)</param>
        /// <param name="AlternateStandardPath">If the non-themed path is not standard, provide the full path here (optional)</param>
        /// <returns></returns>
        public static string GetFinalThemePath(string SiteThemeName, PathType PathType, string ViewName = "", string AlternateStandardPath = "")
        {
            if (SiteThemeName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("No theme has been set for this website root, republish the root with a selected theme.");
            }

            var finalPath = "";
            var standardPath = "";
            var themePath = "";
            var isFolder = false;

            var baseThemePath = string.Format("~/Themes/{0}", SiteThemeName);

            switch (PathType)
            {
                case PathType.ThemeRoot:
                    standardPath = themePath;
					themePath = string.Format("{0}/", baseThemePath);
                    isFolder = true;
                    break;

                case PathType.View:
                    standardPath = AlternateStandardPath !="" ? AlternateStandardPath : string.Format("~/Views/{0}.cshtml", ViewName);
                    themePath = string.Format("{0}/Views/{1}.cshtml", baseThemePath, ViewName);
                    break;

                case PathType.PartialView:
                    standardPath = AlternateStandardPath != "" ? AlternateStandardPath : string.Format("~/Views/Partials/{0}.cshtml", ViewName);
                    themePath = string.Format("{0}/Views/Partials/{1}.cshtml", baseThemePath, ViewName);
                    break;

                case PathType.GridEditor:
                    standardPath = AlternateStandardPath != "" ? AlternateStandardPath : string.Format("~/Views/Partials/Grid/Editors/{0}.cshtml", ViewName);
                    themePath = string.Format("{0}/Views/Partials/Grid/Editors/{1}.cshtml", baseThemePath, ViewName);
                    break;

                default:
                    break;
            }

            if (isFolder & System.IO.Directory.Exists(IOHelper.MapPath(themePath)))
            {
                finalPath = themePath;
            }
            else if (!isFolder & System.IO.File.Exists(IOHelper.MapPath(themePath)))
            {
                finalPath = themePath;
            }
            else
            {
                finalPath = standardPath;
            }

            return finalPath;
        }

        /// <summary>
        /// Shortcut for 'GetFinalThemePath()' with PathType.ThemeRoot
        /// </summary>
        /// <param name="SiteThemeName"></param>
        /// <returns></returns>
        public static string GetThemePath(string SiteThemeName)
        {
            var path = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            return path;
            }

        /// <summary>
        /// Shortcut for 'GetFinalThemePath()' with PathType.View
        /// </summary>
        /// <param name="SiteThemeName"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static string GetThemeViewPath(string SiteThemeName, string ViewName)
        {
            var path = GetFinalThemePath(SiteThemeName, PathType.View, ViewName);
            return path;
            }

        
        /// <summary>
        /// Shortcut for 'GetFinalThemePath()' with PathType.PartialView
        /// </summary>
        /// <param name="SiteThemeName"></param>
        /// <param name="ViewName"></param>
        /// <returns></returns>
        public static string GetThemePartialViewPath(string SiteThemeName, string ViewName)
        {

            var path = GetFinalThemePath(SiteThemeName, PathType.PartialView, ViewName);
            return path;
        }

        public static string GetCssOverridePath(string CssOverrideFileName)
        {
            if (CssOverrideFileName.IsNullOrWhiteSpace())
            {
                return "";
            }
            else
            {
                var path = "/Themes/~CssOverrides/{0}";
                return string.Format(path, CssOverrideFileName);
            }
            
        }

        /// <summary>
        /// Returns the url of a themed asset
        /// ex: @Url.ThemedAsset(Model, "images/favicon.ico")
        /// NOTE: requires '@using ClientDependency.Core.Mvc' in View
        /// </summary>
        /// <param name="Url">UrlHelper</param>
        /// <param name="SiteThemeName"></param>
        /// <param name="RelativeAssetPath">Path to file inside [theme]/Assets/ folder</param>
        /// <returns></returns>
        public static string ThemedAsset(this UrlHelper url, string SiteThemeName, string RelativeAssetPath)
        {
            var themeRoot = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            var absolutePath = VirtualPathUtility.ToAbsolute(themeRoot).EnsureEndsWith('/');
            var virtualPath = absolutePath + "Assets/" + RelativeAssetPath;
            return virtualPath;
        }

        #region HTML Helpers

        public static HtmlHelper RequiresThemedCss(this HtmlHelper html, string SiteThemeName, string FilePath)
        {
            var themeRoot = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            return html.RequiresCss(themeRoot + "Assets/css" + FilePath.EnsureStartsWith('/'));
        }

        public static HtmlHelper RequiresThemedJs(this HtmlHelper html, string SiteThemeName, string FilePath)
        {
            var themeRoot = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            return html.RequiresJs(themeRoot + "Assets/js" + FilePath.EnsureStartsWith('/'));
        }

        public static HtmlHelper RequiresThemedCssFolder(this HtmlHelper html, string SiteThemeName)
        {
            var themeRoot = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            return html.RequiresCssFolder(themeRoot + "Assets/css");
        }

        public static HtmlHelper RequiresThemedJsFolder(this HtmlHelper html, string SiteThemeName)
        {
            var themeRoot = GetFinalThemePath(SiteThemeName, PathType.ThemeRoot);
            return html.RequiresJsFolder(themeRoot + "Assets/js");
        }

        /// <summary>
        /// Renders a partial view in the current theme
        /// </summary>
        /// <param name="html"></param>
        /// <param name="SiteThemeName"></param>
        /// <param name="PartialName"></param>
        /// <param name="ViewModel"></param>
        /// <param name="ViewData"></param>
        /// <returns></returns>
        public static IHtmlString ThemedPartial(this HtmlHelper html, string SiteThemeName, string PartialName, object ViewModel, ViewDataDictionary ViewData = null)
        {
            try
            {
                var path = GetFinalThemePath(SiteThemeName, PathType.PartialView, PartialName); 
                return html.Partial(path, ViewModel, ViewData);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error rendering partial view '{0}'", PartialName);
                LogHelper.Error<IHtmlString>(msg, ex);
                return new HtmlString(string.Format("<span class=\"error\">{0}</span>", msg));
            }
        }

        /// <summary>
        /// Renders a partial view in the current theme
        /// </summary>
        /// <param name="html"></param>
        /// <param name="SiteThemeName"></param>
        /// <param name="PartialName"></param>
        /// <param name="ViewData"></param>
        /// <returns></returns>
        public static IHtmlString ThemedPartial(this HtmlHelper html, string SiteThemeName, string PartialName, ViewDataDictionary ViewData = null)
        {
            if (ViewData == null)
            {
                ViewData = html.ViewData;
            }
            try
            {
                var path = GetFinalThemePath(SiteThemeName,  PathType.PartialView ,PartialName);
                return html.Partial(path, ViewData);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error rendering partial view '{0}'", PartialName);
                LogHelper.Error<IHtmlString>(msg, ex);
                return new HtmlString(string.Format("<span class=\"error\">{0}</span>", msg));
            }

        }

        #endregion
    }

}
