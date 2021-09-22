using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
public class SaveCheck : MonoBehaviour
{
    private const string CHECKNUMBER_PREFS = "CheckNumber";
    public GameObject button;
    [Header("Texts")]
    public TextMeshProUGUI fromText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI checkNumberText;
    public TextMeshProUGUI checkNumberText2;
    public TextMeshProUGUI reasonText;
    public string from = "أحمد غسان عبدالله قدورة";
    public string value = "$2,134,900.00";
    public string reason = "تطبيق للهواتف الذكية يدعى بطاطا و البندورة 5 بمية و الباذنجان غالي و أنا بس قاعد بجرب بال3 أسطر يا حبيبي يا فادي أتمنى لك يوما سعيدًا";
    public RectTransform receiptArea;
    public void Save()
    {
        int checkNumberInt = PlayerPrefs.GetInt(CHECKNUMBER_PREFS, 1);
        checkNumberText.text = checkNumberInt.ToString("00");
        checkNumberText2.text = checkNumberInt.ToString("00");
        fromText.text = from;
        valueText.text = value;
        reasonText.text = reason;
        dateText.text = DateTime.Now.ToString("yyyy/MM/dd");
        PlayerPrefs.SetInt(CHECKNUMBER_PREFS, ++checkNumberInt);
        StartCoroutine(TakeScreenshotAndShare());
    }
    private IEnumerator TakeScreenshotAndShare()
    {
        button.SetActive(false);
        yield return new WaitForEndOfFrame();
        Rect rt = GetRect(receiptArea);
        Texture2D receiptImage = new Texture2D(Mathf.FloorToInt(rt.width), Mathf.FloorToInt(rt.height), TextureFormat.RGB24, false);
        receiptImage.ReadPixels(rt, 0, 0);
        receiptImage.Apply();
        Destroy(receiptImage);
        button.SetActive(true);
        string filename = "receipt_no_" + checkNumberText.text + ".png";
#if UNITY_EDITOR
        string filePath = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(filePath, receiptImage.EncodeToPNG());
        Debug.Log(filePath);
        yield break;
#endif
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(receiptImage, "Receipts", filename, (success, path) =>
        {
            if (success)
            {
                new NativeShare().AddFile(path)
                    .SetSubject("Receipt from " + from).SetText("We recieved " + value + ", from: " + from)
                    .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                    .Share();
            }
        });
        Debug.Log("Permission result: " + permission);
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
}
