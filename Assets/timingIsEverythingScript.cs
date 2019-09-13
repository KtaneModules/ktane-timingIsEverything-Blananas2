using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class timingIsEverythingScript : MonoBehaviour
{

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable Button;
    public GameObject[] Lights;
    public Material GreenMat;
    public TextMesh Text;

    float startTime = 0;
    float alfa = 0;
    float bravo = 0;
    float charlie = 0;
    float timeA = 0;
    float timeB = 0;
    float timeC = 0;
    string strA = "";
    string strB = "";
    string strC = "";
    int boo = 0;
    int stages = 0;
    bool active = false;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        Module.OnActivate += delegate () { ModuleStart(); };
        Button.OnInteract += delegate () { PressButton(); return false; };

    }

    void ModuleStart()
    {

        startTime = Mathf.Floor(Bomb.GetTime());
        alfa = UnityEngine.Random.Range(11, startTime - 10);
        bravo = UnityEngine.Random.Range(11, startTime - 10);
        charlie = UnityEngine.Random.Range(11, startTime - 10);
        alfa = alfa - (alfa % 1);
        bravo = bravo - (bravo % 1);
        charlie = charlie - (charlie % 1);

        //ABC, ACB, BAC, BCA, CAB, CBA
        //ABC : A >= B, B >= C, A >= C
        //lowest: +1, highest: - 1

        Debug.Log("BEFORE: " + alfa + " " + bravo + " " + charlie);

        if (alfa >= bravo && bravo >= charlie && alfa >= charlie)
        {
            timeA = alfa;
            timeB = bravo;
            timeC = charlie;
        }
        else if (alfa >= charlie && charlie >= bravo && alfa >= bravo)
        {
            timeA = alfa;
            timeB = charlie;
            timeC = bravo;
        }
        else if (bravo >= alfa && alfa >= charlie && bravo >= charlie)
        {
            timeA = bravo;
            timeB = alfa;
            timeC = charlie;
        }
        else if (bravo >= charlie && charlie >= alfa && bravo >= alfa)
        {
            timeA = bravo;
            timeB = charlie;
            timeC = alfa;
        }
        else if (charlie >= alfa && alfa >= bravo && charlie >= bravo)
        {
            timeA = charlie;
            timeB = alfa;
            timeC = bravo;
        }
        else if (charlie >= bravo && bravo >= alfa && charlie >= alfa)
        {
            timeA = charlie;
            timeB = bravo;
            timeC = alfa;
        }
        else
        {
            Debug.Log("f.");
        }

        timeA += 5;
        timeC -= 5;

        Debug.Log("AFTER: " + timeA + " " + timeB + " " + timeC);

        GenerateStrings(timeA, strA, 1);
        GenerateStrings(timeB, strB, 2);
        GenerateStrings(timeC, strC, 3);

        Text.text = strA;

        Debug.LogFormat("[Timing is Everything #{0}] The times are: {1}, {2}, and {3}.", moduleId, strA, strB, strC);
    }

    void GenerateStrings(float t, string s, int n)
    {
        charlie = t % 60;
        bravo = ((t - charlie) % 3600) / 60;
        alfa = (t - (bravo * 60 + charlie)) / 3600;

        if (bravo > 9 && charlie > 9)
        {
            s = alfa + ":" + bravo + ":" + charlie;
        }
        else if (bravo < 10 && charlie > 9)
        {
            s = alfa + ":0" + bravo + ":" + charlie;
        }
        else if (bravo > 9 && charlie < 10)
        {
            s = alfa + ":" + bravo + ":0" + charlie;
        }
        else
        {
            s = alfa + ":0" + bravo + ":0" + charlie;
        }

        if (n == 1)
        {
            strA = s;
        }
        else if (n == 2)
        {
            strB = s;
        }
        else if (n == 3)
        {
            strC = s;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (stages == 0)
        {
            if (Mathf.Floor(Bomb.GetTime()) < timeA)
            {
                GetComponent<KMBombModule>().HandleStrike();
                timeA = ((timeA + timeB) / 2) - ((timeA + timeB) / 2) % 1;
                GenerateStrings(timeA, strA, 1);
                Text.text = strA;
                Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 1 time. Strike! New time: {1}", moduleId, strA);
            }
        }
        else if (stages == 1)
        {
            if (Mathf.Floor(Bomb.GetTime()) < timeB)
            {
                GetComponent<KMBombModule>().HandleStrike();
                timeB = ((timeB + timeC) / 2) - ((timeB + timeC) / 2) % 1;
                GenerateStrings(timeB, strB, 2);
                Text.text = strB;
                Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 2 time. Strike! New time: {1}", moduleId, strB);
            }
        }
        else if (stages == 2)
        {
            if (Mathf.Floor(Bomb.GetTime()) < timeC)
            {
                GetComponent<KMBombModule>().HandleStrike();
                timeC = (timeC / 2) - (timeC / 2) % 1;
                GenerateStrings(timeC, strC, 3);
                Text.text = strC;
                Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 3 time. Strike! New time: {1}", moduleId, strC);
            }
        }

        if (startTime - Mathf.Floor(Bomb.GetTime()) > 1)
        {
            active = true;
        }

        if (timeA == timeB && active == true && stages == 0)
        {
            stages = 1;
            Text.text = strB;
            Lights[0].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 1 automatically complete due to time becoming the same as Stage 2. {1} {2} {3}", moduleId, strA, strB, strC);
        }
        if (timeB == timeC && active == true && stages == 1)
        {
            stages = 2;
            Text.text = strC;
            Lights[1].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 2 automatically complete due to time becoming the same as Stage 3.", moduleId, strA, strB, strC);
        }
        if (timeC < 5 && active == true && stages == 2)
        {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            stages = 3;
            Text.text = "!!!";
            Lights[2].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 3 automatically complete due to time becoming under 5 seconds. Module Solved.", moduleId, strA, strB, strC);
        }
    }

    void PressButton()
    {
        Button.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (stages == 0 && Mathf.Floor(Bomb.GetTime()) == timeA)
        {
            stages = 1;
            Text.text = strB;
            Lights[0].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 1 complete.", moduleId);
        }
        else if (stages == 1 && Mathf.Floor(Bomb.GetTime()) == timeB)
        {
            stages = 2;
            Text.text = strC;
            Lights[1].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 2 complete.", moduleId);
        }
        else if (stages == 2 && Mathf.Floor(Bomb.GetTime()) == timeC)
        {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            stages = 3;
            Text.text = "!!!";
            Lights[2].GetComponent<MeshRenderer>().material = GreenMat;
            Debug.LogFormat("[Timing is Everything #{0}] Stage 3 complete, module solved.", moduleId);

        }
        else if (stages != 3)
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Timing is Everything #{0}] Too early on Stage {1}. Strike!", moduleId, stages + 1);
        }
    }

    //twitch plays
    private bool timeIsValid(string s)
    {
        Regex timeRegex1 = new Regex(@"[0-9][0-9]");
        Regex timeRegex2 = new Regex(@"[0-9][:][0-9][0-9]");
        Regex timeRegex3 = new Regex(@"[0-9][0-9][:][0-9][0-9]");
        Regex timeRegex4 = new Regex(@"[0-9][:][0-9][0-9][:][0-9][0-9]");
        Match match = timeRegex1.Match(s);
        Match match2 = timeRegex2.Match(s);
        Match match3 = timeRegex3.Match(s);
        Match match4 = timeRegex4.Match(s);
        if (match.Success && s.Length == 2)
        {
            return true;
        }
        else if (match2.Success && s.Length == 4)
        {
            return true;
        }
        else if (match3.Success && s.Length == 5)
        {
            return true;
        }
        else if (match4.Success && s.Length == 7)
        {
            return true;
        }
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit <time> [Presses the submit button at the specified time] | Supported time formats: ##, #:##, ##:##, #:##:##";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (timeIsValid(parameters[1]))
                {
                    yield return null;
                    if (parameters[1].Length == 2)
                    {
                        parameters[1] = "00:" + parameters[1];
                    }
                    else if (parameters[1].Length == 4)
                    {
                        parameters[1] = "0" + parameters[1];
                    }
                    else if(parameters[1].Length == 7)
                    {
                        int temp = 0;
                        int temp2 = 0;
                        int.TryParse(parameters[1].Substring(0, 1), out temp);
                        temp *= 60;
                        int.TryParse(parameters[1].Substring(2, 2), out temp2);
                        temp += temp2;
                        string tem = "" + temp;
                        tem += parameters[1].Substring(4, 3);
                        parameters[1] = tem;
                    }
                    yield return "sendtochat Submit time set for '" + parameters[1] + "'";
                    if ((int)Bomb.GetTime() < 60)
                    {
                        int temp = 0;
                        int.TryParse(parameters[1].Substring(parameters[1].Length - 2, 2), out temp);
                        while ((int)Bomb.GetTime() != temp) yield return "trycancel The submit button's press was cancelled due to a cancel request.";
                    }
                    else
                    {
                        while (!Bomb.GetFormattedTime().Equals(parameters[1])) yield return "trycancel The submit button's press was cancelled due to a cancel request.";
                    }
                    Button.OnInteract();
                }
            }
            yield break;
        }
    }
}