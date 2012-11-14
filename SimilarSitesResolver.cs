using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;

using Sitecore;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.Web;
using SiteMapper.Sites;
using Sitecore.Diagnostics;

namespace SiteMapper.Pipelines.HttpRequest
{
   internal class SimilarSitesResolver
   {
      private static List<string> officialSites = new List<string>();

      static SimilarSitesResolver()
      {
         officialSites.Add("shell");
         officialSites.Add("login");
         officialSites.Add("testing");
         officialSites.Add("admin");
         officialSites.Add("modules_shell");
         officialSites.Add("modules_website");
      }

      public void Process(HttpRequestArgs args)
      {
         SiteContext site = null;
         string language = Sitecore.Configuration.Settings.DefaultLanguage;
         if (!isOfficialSite(Sitecore.Context.Site.Name, args))
         {
            if (Sitecore.Context.Site.Name == "website")
            {
               KeyValuePair<SiteContext, string> siteCI = this.ResolveSimilarSiteContext(args);
               site = siteCI.Key;
               language = siteCI.Value;

               if (site != null)
               {
                  this.UpdatePaths(args, site, Sitecore.Context.Site);
                  Sitecore.Context.Site = site;
                  Sitecore.Context.Language = Sitecore.Globalization.Language.Parse(string.IsNullOrEmpty(language) ? Sitecore.Configuration.Settings.DefaultLanguage : language);
               }
            }
         }
      }

      #region Resolver helpers

      private KeyValuePair<SiteContext, string> ResolveSimilarSiteContext(HttpRequestArgs args)
      {
         string currentHostName = Sitecore.Web.WebUtil.GetHostName();
         ArrayList sites = SimilarSites.Sites;
         for (int i = 0; i < sites.Count; i++)
         {
            SiteInfo info = sites[i] as SiteInfo;
            if (info.HostName == currentHostName)
            {
               SiteContext siteContext = SiteContextFactory.GetSiteContext(info.Name);
               string language = info.Language;
               return new KeyValuePair<SiteContext, string>(siteContext, language);
            }
         }
         return new KeyValuePair<SiteContext, string>(null, null);
      }

      private void UpdatePaths(HttpRequestArgs args, SiteContext site, SiteContext currentSite)
      {
         args.StartPath = site.StartPath;
         args.ItemPath = this.GetItemPath(args, site, currentSite);
         site.Request.ItemPath = args.ItemPath;
         args.FilePath = this.GetFilePath(args, site, currentSite);
         site.Request.FilePath = args.FilePath;
      }

      private string GetFilePath(HttpRequestArgs args, SiteContext site, SiteContext currentSite)
      {
         return this.GetPath(site.PhysicalFolder, args.FilePath.Replace(currentSite.PhysicalFolder, ""), site);
      }

      private string GetItemPath(HttpRequestArgs args, SiteContext site, SiteContext currentSite)
      {
         return this.GetPath(site.StartPath, args.ItemPath.Replace(currentSite.StartPath, ""), site);
      }

      private string GetPath(string basePath, string path, SiteContext context)
      {
         string virtualFolder = context.VirtualFolder;
         if ((virtualFolder.Length > 0) && (virtualFolder != "/"))
         {
            path = path.Substring(virtualFolder.Length);
         }
         if ((basePath.Length > 0) && (basePath != "/"))
         {
            path = Sitecore.IO.FileUtil.MakePath(basePath, path, '/');
         }
         if ((path.Length > 0) && (path[0] != '/'))
         {
            path = '/' + path;
         }
         return path;
      }

      private bool isOfficialSite(string siteName, HttpRequestArgs args)
      {
         foreach (string officialSite in officialSites)
         {
            if (siteName.Contains(officialSite))
            {
               return true;
            }
         }
         if (args.LocalPath == "/sitecore/default")
         {
            return true;
         }
         return false;
      }

      #endregion Resolver helpers
   }
}
