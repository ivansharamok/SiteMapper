using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;

using Sitecore;
using Sitecore.Web;

namespace SiteMapper.Sites
{
   internal static class SimilarSites
   {
      static ArrayList f_sites;

      static SimilarSites()
      {
         Sitecore.Configuration.ConfigWatcher.ConfigChanged += new EventHandler<Sitecore.Configuration.ConfigChangedEventArgs>(ConfigWatcher_ConfigChanged);
      }

      #region Sites watcher

      static void ConfigWatcher_ConfigChanged(object sender, Sitecore.Configuration.ConfigChangedEventArgs e)
      {
         f_sites = null;
      }

      #endregion Sites watcher

      public static ArrayList Sites
      {
         get
         {
            return GetSites();
         }
      }

      private static ArrayList GetSites()
      {
         ArrayList sites = f_sites;
         if (sites == null)
         {
            sites = new ArrayList();
            XmlNodeList configSites = GetSitesNodes("sites/site");
            List<string> hostNames = new List<string>();
            if (configSites != null)
            {
               foreach (XmlNode node in configSites)
               {
                  SiteInfo info = new SiteInfo(node);
                  if (info.IsActive)
                  {
                     sites.Add(info);
                  }
               }
            }
            f_sites = sites;
         }
         return sites;
      }

      private static XmlNodeList GetSitesNodes(string xpath)
      {
         string folder = "/App_Config";
         string fileName = "similar.sites.config";
         XmlDocument document = Sitecore.Xml.XmlUtil.LoadXmlFile(Sitecore.IO.FileUtil.MapPath(Sitecore.IO.FileUtil.MakePath(folder, fileName, '/')));
         if (document.ChildNodes.Count > 0)
         {
            return document.SelectNodes(xpath);
         }
         return null;
      }

   }
}
