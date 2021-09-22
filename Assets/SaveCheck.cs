using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
public class SaveCheck : MonoBehaviour
{
    private const string CHECKNUMBER_PREFS = "CheckNumber";
    public GameObject saveButton;
    public GameObject shareButton;
    [Header("Texts")]
    public TextMeshProUGUI fromText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI checkNumberText;
    public TextMeshProUGUI checkNumberText2;
    public TextMeshProUGUI reasonText;
    public string from = "";
    public string value = "";
    public string reason = "";
    public RectTransform receiptArea;
    private string filePath = "";
    int checkNumberInt = 1;
    public void Save()
    {
        PlayerPrefs.SetInt(CHECKNUMBER_PREFS, ++checkNumberInt);
        StartCoroutine(TakeScreenshotAndShare());
    }
    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();
        Rect rt = GetRect(receiptArea);
        Texture2D receiptImage = new Texture2D(Mathf.FloorToInt(rt.width), Mathf.FloorToInt(rt.height), TextureFormat.RGB24, false);
        receiptImage.ReadPixels(rt, 0, 0);
        receiptImage.Apply();
        Destroy(receiptImage);
        string filename = "receipt_no_" + checkNumberText.text + ".png";
#if UNITY_EDITOR
        filePath = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(filePath, receiptImage.EncodeToPNG());
        saveButton.SetActive (false);
        shareButton.SetActive (true);
#else
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(receiptImage, "Receipts", filename, (success, path) =>
        {
            if (success)
            {
                filePath = path;
                saveButton.SetActive (false);
                shareButton.SetActive (true);
            }
        });
        Debug.Log("Permission result: " + permission);
#endif
    }
    public void ShareBtn () {
        if (string.IsNullOrEmpty (filePath))
            return;
#if UNITY_EDITOR
        Debug.Log (filePath);
#else
        new NativeShare ().AddFile (filePath)
                    .SetSubject ("سند قبض من" + from).SetText ("استلمنا مبلغ " + value + "، من:" + from)
                    .SetCallback ((result, shareTarget) => Debug.Log ("Share result: " + result + ", selected app: " + shareTarget))
                    .Share ();
#endif
    }
    public void SetTexts () {
        checkNumberInt = PlayerPrefs.GetInt (CHECKNUMBER_PREFS, 1);
        checkNumberText.text = checkNumberInt.ToString ("00");
        checkNumberText2.text = checkNumberInt.ToString ("00");
        fromText.text = from;
        valueText.text = value;
        reasonText.text = reason;
        saveButton.SetActive (true);
        shareButton.SetActive (false);
        dateText.text = DateTime.Now.ToString ("yyyy/MM/dd");
    }
    Rect GetRect(RectTransform rectTransform)
    {
        Vector3[] vectors = new Vector3[4];
        rectTransform.GetWorldCorners(vectors);
        vectors.ToList().ForEach(point => {
            point = Camera.main.WorldToScreenPoint(point);
            point.y = Screen.height - point.y; 
        });
        return new Rect(vectors[0].x, vectors[0].y, Vector2.Distance(vectors[0], vectors[3]), Vector2.Distance(vectors[0], vectors[1]));
    }
    public void SetReceiptValue(string value)
    {
        this.value = value;
    }
    public void SetReceipantName(string value)
    {
        from = value;
    }
    public void SetReasonValue (string value) {
        reason = value;
    }
}
