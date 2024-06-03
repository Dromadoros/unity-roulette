using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UnityAnalytics : MonoBehaviour
{
        async void Awake()
        {
            try
            {
                InitializationOptions options = new InitializationOptions();
 
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            options.SetEnvironmentName("dev");
#else
                options.SetEnvironmentName("production");
#endif
                await UnityServices.InitializeAsync(options);
                GiveConsent(); //Get user consent according to various legislations
            }
            catch (ConsentCheckException e)
            {
                Debug.Log(e.ToString());
            }
        }

        public void GiveConsent()
        {
            // Call if consent has been given by the user
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log($"Consent has been provided. The SDK is now collecting data!");
        }

        public void ClickNextStepAnalytics(string parameter)
        {
            // Define Custom Parameters
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "stepName", parameter}
            };
            AnalyticsService.Instance.CustomData("clickNextStep", parameters);

            // You can call Events.Flush() to send the event immediately
            AnalyticsService.Instance.Flush();
        }

}
