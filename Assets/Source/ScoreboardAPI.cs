using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading;
using Assets.Source;
using System.IO;
using System;
using System.Text;

public class ScoreboardAPI : MonoBehaviour
{
    public TMP_InputField ServerUrl;
    //send
    public TMP_InputField Username;
    public TMP_InputField Userscore;
    //load
    public TMP_Text       Scoreboard;
    //
    public string         url;
    static string         api = "/api/Scoreboard";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateScoreBoard());
    }

    public IEnumerator UpdateScoreBoard(){
        while (true)
        {
            yield return new WaitForSeconds(5f);
            StartCoroutine(GetScores());
        }
    }

    public IEnumerator GetScores(){
        url = ((ServerUrl.text == "" ) ? "https://localhost:7264" : ServerUrl.text) + api;
        using (UnityWebRequest uwr = UnityWebRequest.Get(url)) {
            var cert = new ForceAcceptAll();
            uwr.certificateHandler = cert;
            yield return uwr.SendWebRequest();
            switch(uwr.result){
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("Error While Sending: " + uwr.error);
                    break;
                default:
                    Score[] scoreArr = JsonHelper.FromJson<Score>(JsonHelper.fixJson(uwr.downloadHandler.text));
                    StartCoroutine(JsonToScore(scoreArr));
                    break;
            }
            cert?.Dispose();
        }
    }

    public IEnumerator JsonToScore(Score[] scores){
        Scoreboard.text = "";
        Debug.Log(scores);
        for (int i = 0; i < scores.Length; i++)
        {
            yield return new WaitForSeconds(0.2f);
            Scoreboard.text += (i+1) + ". " + scores[i].Name + " : " + scores[i].Score + "\n";
        }
    }

    public void NewScore(){
        StartCoroutine(PostScore());
    }

    public IEnumerator PostScore(){
        url = ((ServerUrl.text == "" ) ? "https://localhost:7264" : ServerUrl.text) + api;
        
        Score score = new Score();
        score.Name = Username.text;
        score.Score = int.Parse(Userscore.text);
        string json = JsonUtility.ToJson(score);

        using (UnityWebRequest uwr = UnityWebRequest.Post(url, json)){
            //for some reason, the .Post(url, json <- ) doesnt work. So we need to use the
            // uploadhandler to actually send the json
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.SetRequestHeader("Content-Type", "application/json");

            var cert = new ForceAcceptAll();
            uwr.certificateHandler = cert;
            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            switch(uwr.result){
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("Error While Sending: " + uwr.error);
                    break;
                default:
                    Debug.Log("sent, return:" + uwr.downloadHandler.text);
                    break;
            }
            cert?.Dispose();
        }
    }

}

//since we are not validating certs on this example, we just skip it
public class ForceAcceptAll : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
