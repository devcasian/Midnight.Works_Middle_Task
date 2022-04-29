using UnityEngine;
using UnityEngine.Purchasing;

public class Purchaser : MonoBehaviour
{
    public void OnPurchaseCompleted(Product product)
    {
        switch (product.definition.id)
        {
            case "coins.100":
                Add100Coins();
                break;
            case "coins.200":
                Add200Coins();
                break;
            case "coins.500":
                Add500Coins();
                break;
            case "coins.1200":
                Add1200Coins();
                break;
            case "coins.2100":
                Add2100Coins();
                break;
        }
    }

    private void Add100Coins()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coin += 100;
        PlayerPrefs.SetInt("coins", coin);
        Debug.Log("Purchase: 100 coins");
        UICoinInfo.Instance.UpdateCoinsText();
    }

    private void Add200Coins()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coin += 200;
        PlayerPrefs.SetInt("coins", coin);
        Debug.Log("Purchase: 200 coins");
        UICoinInfo.Instance.UpdateCoinsText();
    }

    private void Add500Coins()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coin += 500;
        PlayerPrefs.SetInt("coins", coin);
        Debug.Log("Purchase: 500 coins");
        UICoinInfo.Instance.UpdateCoinsText();
    }

    private void Add1200Coins()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coin += 1200;
        PlayerPrefs.SetInt("coins", coin);
        Debug.Log("Purchase: 1200 coins");
        UICoinInfo.Instance.UpdateCoinsText();
    }

    private void Add2100Coins()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coin += 2100;
        PlayerPrefs.SetInt("coins", coin);
        Debug.Log("Purchase: 2100 coins");
        UICoinInfo.Instance.UpdateCoinsText();
    }
}