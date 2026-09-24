using Clinic.Core.Data;
using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Clinic.Web.Infrastructure.SiteMap
{
    public class ClinicVisibilityProvider : SiteMapNodeVisibilityProviderBase
    {
        private static readonly object lockObject = new object();
        private static Dictionary<int, List<string>> screenRoleCache;
        private static List<int> activeScreens;
        private static bool isInitialized = false;

        private void EnsureCache()
        {
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        try
                        {
                            using (var db = new Entities())
                            {
                                activeScreens = db.Screens
                                    .Where(s => s.IsActive && !s.IsDeleted)
                                    .Select(s => s.Id)
                                    .ToList();

                                var permissions = db.RoleScreenPermissions
                                    .Include("AspNetRole")
                                    .Where(p => p.ScreenShow && p.AspNetRole != null)
                                    .ToList();

                                screenRoleCache = new Dictionary<int, List<string>>();

                                foreach (var perm in permissions)
                                {
                                    if (!screenRoleCache.ContainsKey(perm.ScreenId))
                                    {
                                        screenRoleCache[perm.ScreenId] = new List<string>();
                                    }
                                    screenRoleCache[perm.ScreenId].Add(perm.AspNetRole.Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("SiteMap Cache Error: " + ex.Message);
                            screenRoleCache = new Dictionary<int, List<string>>();
                            activeScreens = new List<int>();
                        }
                        finally
                        {
                            isInitialized = true;
                        }
                    }
                }
            }
        }

        public override bool IsVisible(ISiteMapNode node, IDictionary<string, object> sourceMetadata)
        {
            if (string.IsNullOrEmpty(node.Key) || node.Key == "Root")
                return true;

            EnsureCache();

            int screenId;
            if (int.TryParse(node.Key, out screenId))
            {
                if (activeScreens != null && !activeScreens.Contains(screenId))
                {
                    return false;
                }

                if (screenRoleCache != null && screenRoleCache.ContainsKey(screenId))
                {
                    var context = HttpContext.Current;
                    if (context != null && context.User != null)
                    {
                        foreach (var role in screenRoleCache[screenId])
                        {
                            if (context.User.IsInRole(role))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            // If it's a parent node, we check if any child is visible.
            if (node.HasChildNodes)
            {
                foreach (ISiteMapNode child in node.ChildNodes)
                {
                    if (IsVisible(child, sourceMetadata))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
