using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading;
using Assets.Source;
using System.IO;
using System;

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
        Debug.Log(url);
        using (UnityWebRequest uwr = UnityWebRequest.Get(url)) {
            var cert = new ForceAcceptAll();
            uwr.certificateHandler = cert;
            yield return uwr.SendWebRequest();
            switch(uwr.result){
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("Error While Sending: " + uwr.error);
                    break;
                default:
                    Score[] scoreArr = JsonUtility.FromJson<Score[]>(uwr.downloadHandler.text);
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
            Scoreboard.text += (i+1) + ". " + scores[i].name + " : " + scores[i].score + "\n";
        }
    }

    public void NewScore(){
        StartCoroutine(PostScore());
    }

    public IEnumerator PostScore(){
        url = ((ServerUrl.text == "" ) ? "https://localhost:7264" : ServerUrl.text) + api;
        Debug.Log(url);
        Score score = new Score();
        score.name = Username.text;
        score.score = int.Parse(Userscore.text);
        Debug.Log(JsonUtility.ToJson(score));
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, JsonUtility.ToJson(score))){
            var cert = new ForceAcceptAll();
            uwr.certificateHandler = cert;
            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            switch(uwr.result){
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("Error While Sending: " + uwr.error);
                    break;
                default:
                    Debug.Log("New score sent");
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