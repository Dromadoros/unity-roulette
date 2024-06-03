using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct ItemCasino
    {
        public Sprite sprite;

        // Chance from 0.01 to 1.00
        public float chance;
        public string title;

        // Constructor
        public ItemCasino(Sprite sprite, float chance, string title)
        {
            this.sprite = sprite;
            this.chance = chance;
            this.title = title;
        }
    }

    [SerializeField] public List<ItemCasino> itemsCasino;
    public GameObject itemTemplateSlot1;
    public GameObject itemTemplateSlot2;
    public GameObject itemTemplateSlot3;
    public string urlToPromote;
    public GameObject casinoGameObject;
    public GameObject modalReview;
    public GameObject readyButton;
    public GameObject playButton;
    public Button openUrlButton;
    public InputField inputFirstname;
    public InputField inputEmail;
    public Button buttonEmail;
    public GameObject modalEmail;
    public GameObject modalEnd;
    public Text finalItemTextWinTitle;
    public GameObject winParticles;
    public Material winMaterial;
    public GameObject legalLinks;
    public Animator starsAnimator;
    public GameObject cannotPlayText;
    public GameObject[] stars;
    public string xApiKey = "API_KEY_FROM_MAILJET";
    public int templateID = 0;
    public string awsLambdaUrl = "YOUR_AWS_LAMBDA_URL";
    public string clientEmail;

    private ItemCasino itemWin;

    private const string PLAYER_PREF_LAST_TIME_PLAYED = "last_time_played";

    private void Awake()
    {
        modalReview.SetActive(false);
        casinoGameObject.SetActive(true);
        modalEmail.SetActive(false);
        openUrlButton.interactable = true;
        playButton.SetActive(false);
        readyButton.SetActive(true);
        winParticles.SetActive(false);
        cannotPlayText.SetActive(false);
        buttonEmail.interactable = false;
        legalLinks.SetActive(true);
        
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
    }

    void Start()
    {
        itemWin = GetRandomItem();
        finalItemTextWinTitle.text = itemWin.title;
        winMaterial.SetTexture("_MainTex", itemWin.sprite.texture);
        Debug.Log(itemWin.title);
        FillSlot(itemTemplateSlot1);
        FillSlot(itemTemplateSlot2);
        FillSlot(itemTemplateSlot3);
        DateTime currentUtcTime = DateTime.UtcNow;
        int unixTimestamp = (int)((DateTimeOffset)currentUtcTime).ToUnixTimeSeconds();
        int lastTimestamp = PlayerPrefs.GetInt(PLAYER_PREF_LAST_TIME_PLAYED, 0);
        // Convert Unix timestamps to DateTime objects
        DateTime dateTime1 = DateTimeOffset.FromUnixTimeSeconds(lastTimestamp).UtcDateTime;
        DateTime dateTime2 = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;

        TimeSpan difference = dateTime2 - dateTime1;

        if (difference.TotalDays < 6)
        {
            readyButton.GetComponent<Button>().interactable = false;
            readyButton.SetActive(false);
            cannotPlayText.SetActive(true);
        }

        foreach (GameObject star in stars)
        {
            StartCoroutine(PopStar(star));
        }
    }

    bool IsValidEmail(string email)
    {
        // Regular expression for email validation
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // Check if email matches the pattern
        return Regex.IsMatch(email, emailPattern);
    }

    public void OnEmailNameType(string inputText)
    {
        buttonEmail.interactable = IsValidEmail(inputEmail.text) && inputFirstname.text.Length > 0;
    }

    IEnumerator PopStar(GameObject star)
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        star.SetActive(true);
    }

    private void FillSlot(GameObject itemTemplateSlot)
    {
        int count = 0;

        for (int i = 0; i < 30; i++)
        {
            ShuffleItems();
            foreach (ItemCasino item in itemsCasino)
            {
                count += 1;
                GameObject clonedObject = Instantiate(itemTemplateSlot, itemTemplateSlot.transform.parent);
                if (count == 75)
                {
                    clonedObject.GetComponent<Image>().sprite = itemWin.sprite;
                    clonedObject.name = "item-" + itemWin.title + "-" + count;
                }
                else
                {
                    clonedObject.GetComponent<Image>().sprite = item.sprite;
                    clonedObject.name = "item-" + item.title + "-" + count;
                }

                clonedObject.SetActive(true);
            }
        }
    }

    public ItemCasino GetRandomItem()
    {
        List<ItemCasino> probabilityItems = new List<ItemCasino>();

        foreach (ItemCasino itemCasino in itemsCasino)
        {
            for (int i = 0; i < itemCasino.chance; i++)
            {
                probabilityItems.Add(itemCasino);
            }
        }

        ItemCasino randomItem = probabilityItems.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        return randomItem;
    }

    // Method to shuffle the list
    public void ShuffleItems()
    {
        System.Random rng = new System.Random();
        int n = itemsCasino.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (itemsCasino[k], itemsCasino[n]) = (itemsCasino[n], itemsCasino[k]);
        }
    }

    public void ClickPlay()
    {
        StartCoroutine(PlayRouletteTimer(0, itemTemplateSlot1));
        StartCoroutine(PlayRouletteTimer(0.3f, itemTemplateSlot2));
        StartCoroutine(PlayRouletteTimer(0.6f, itemTemplateSlot3));
        StartCoroutine(PlayWaitTime());
    }

    private IEnumerator PlayRouletteTimer(float seconds, GameObject itemTemplate)
    {
        yield return new WaitForSeconds(seconds);
        itemTemplate.transform.parent.GetComponent<Animator>().SetTrigger("Play");
    }

    public void ClickReadyButton()
    {
        modalReview.SetActive(true);
    }

    public void ClickOpenUrlToPromote()
    {
        StartCoroutine(WaitToHideModalReview());
    }

    public void ClickReceiveGift()
    {
        modalEmail.SetActive(false);
        modalEnd.SetActive(true);
        
        // Define Custom Parameters
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "itemName", itemWin.title}
        };
        AnalyticsService.Instance.CustomData("rewardItem", parameters);

        // You can call Events.Flush() to send the event immediately
        AnalyticsService.Instance.Flush();
        // Send to the user.
        StartCoroutine(SendOptionsRequest(inputEmail.text));
        StartCoroutine(SendOptionsRequest(clientEmail, true));
    }

    public IEnumerator WaitToHideModalReview()
    {
        Application.OpenURL(urlToPromote);
        starsAnimator.SetTrigger("Play");
        readyButton.SetActive(false);
        openUrlButton.interactable = false;
        yield return new WaitForSeconds(1);
        modalReview.SetActive(false);
        playButton.SetActive(true);
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public IEnumerator PlayWaitTime()
    {
        yield return new WaitForSeconds(5.6f);
        casinoGameObject.SetActive(false);
        winParticles.SetActive(true);
        modalEmail.SetActive(true);
        DateTime currentUtcTime = DateTime.UtcNow;
        int unixTimestamp = (int)((DateTimeOffset)currentUtcTime).ToUnixTimeSeconds();
        legalLinks.SetActive(false);
        PlayerPrefs.SetInt(PLAYER_PREF_LAST_TIME_PLAYED, unixTimestamp);
        PlayerPrefs.Save();
    }
    
    IEnumerator SendOptionsRequest(string toEmail, bool isClient = false)
    {
        string toName = inputFirstname.text;
        string winPrice = itemWin.title;

        // Constructing JSON payload
        Dictionary<string, object> message = new Dictionary<string, object>();
        List<Dictionary<string, object>> to = new List<Dictionary<string, object>>();
        Dictionary<string, object> variables = new Dictionary<string, object>();

        Dictionary<string, object> toRecipient = new Dictionary<string, object>();
        toRecipient.Add("Email", toEmail);
        toRecipient.Add("Name", toName);
        to.Add(toRecipient);

        variables.Add("WIN_PRICE", winPrice);

        message.Add("To", to);

        if (isClient)
        {
            message.Add("Subject", inputEmail.text + " a gagné : " + itemWin.title);
        }
        message.Add("TemplateID", templateID);
        message.Add("TemplateLanguage", true);
        message.Add("Variables", variables);

        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload.Add("Messages", new object[] { message });

        string payloadJson = JsonConvert.SerializeObject(payload);

        // Setting up request headers
        UnityWebRequest request = new UnityWebRequest(awsLambdaUrl, "OPTIONS");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", xApiKey);

        // Send the request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Options request sent successfully");
            StartCoroutine(SendMail(payloadJson));
        }
        else
        {
            Debug.LogError("Error sending options request: " + request.error);
        }
    }

    IEnumerator SendMail(string payloadJson)
    {
        // Setting up request headers
        UnityWebRequest request = new UnityWebRequest(awsLambdaUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payloadJson);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", xApiKey);

        // Send the request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Mail sent successfully.");
        }
        else
        {
            Debug.LogError("Error sending mail: " + request.error);
        }
    }

    public void CloseModal()
    {
        modalReview.SetActive(false);
    }
}