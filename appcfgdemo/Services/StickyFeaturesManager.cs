using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using System.Linq;
using System.Threading.Tasks;

namespace appcfgdemo.Services
{
    public class StickyFeaturesManager : ISessionManager
    {
        private const string STICKY_FEATURES_POSTFIX = ".Sticky";
        private const string FEATURE_ENABLED = "1";
        private const string FEATURE_DISABLED = "0";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IFeatureDefinitionProvider _featureDefinitionProvider;

        public StickyFeaturesManager(
            IHttpContextAccessor contextAccessor,
            IFeatureDefinitionProvider featureDefinitionProvider)
        {
            _contextAccessor = contextAccessor;
            _featureDefinitionProvider = featureDefinitionProvider;
        }

        public async Task<bool?> GetAsync(string featureName)
        {
            if (featureName.EndsWith(STICKY_FEATURES_POSTFIX))
            {
                string cookieName = $"feat_{featureName}";
                if (_contextAccessor.HttpContext.Request.Cookies.ContainsKey(cookieName))
                {
                    if (await IsFeatureEnabled(featureName))
                        return _contextAccessor.HttpContext.Request.Cookies[cookieName] == FEATURE_ENABLED;
                    else
                    {
                         _contextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
                        return null;
                    }
                }
            }
            return null;
        }

        public async Task SetAsync(string featureName, bool enabled)
        {
            if (featureName.EndsWith(STICKY_FEATURES_POSTFIX))
            {
                string cookieName = $"feat_{featureName}";

                if (await IsFeatureEnabled(featureName))
                {
                    _contextAccessor.HttpContext.Response.Cookies.Append(cookieName, enabled ? FEATURE_ENABLED : FEATURE_DISABLED, new CookieOptions()
                    {
                        // Set "Expires" for a persistent Cookie
                    });
                }
                else
                {
                    _contextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
                }
            }
        }

        private async Task<bool> IsFeatureEnabled(string featureName)
        {
            FeatureDefinition featureDefinition = await _featureDefinitionProvider.GetFeatureDefinitionAsync(featureName);
            return featureDefinition.EnabledFor.Count() > 0;
        }
    }
}
