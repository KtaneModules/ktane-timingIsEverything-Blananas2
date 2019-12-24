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
    public Material[] buttonColors;

    bool moduleReady = false;
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

    bool TimeModeActive;
    bool TwitchPlaysSkipTimeAllowed = true;
    bool timeMode = false;

    bool ZenModeActive;
    bool zenMode = false;

    bool SteadyModeActive;
    bool steadyMode = false;

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
        StartCoroutine(delay());

        startTime = Mathf.Floor(Bomb.GetTime());
        alfa = UnityEngine.Random.Range(11, startTime - 10);
        bravo = UnityEngine.Random.Range(11, startTime - 10);
        charlie = UnityEngine.Random.Range(11, startTime - 10);
        alfa = Math.Abs(alfa - (alfa % 1));
        bravo = Math.Abs(bravo - (bravo % 1));
        charlie = Math.Abs(charlie - (charlie % 1));

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
        if(timeMode == false && zenMode == false) // && steadyMode == false
        {
            if (stages == 0)
            {
                if (Mathf.Floor(Bomb.GetTime()) < timeA)
                {
                    if (moduleReady == true) {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 1 time. Strike! New time: {1}", moduleId, strA);
                    }
                    timeA = ((timeA + timeB) / 2) - ((timeA + timeB) / 2) % 1;
                    GenerateStrings(timeA, strA, 1);
                    Text.text = strA;
                }
            }
            else if (stages == 1)
            {
                if (Mathf.Floor(Bomb.GetTime()) < timeB)
                {
                    if (moduleReady == true)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 2 time. Strike! New time: {1}", moduleId, strB);
                    }
                    timeB = ((timeB + timeC) / 2) - ((timeB + timeC) / 2) % 1;
                    GenerateStrings(timeB, strB, 2);
                    Text.text = strB;
                }
            }
            else if (stages == 2)
            {
                if (Mathf.Floor(Bomb.GetTime()) < timeC)
                {
                    if (moduleReady == true)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 3 time. Strike! New time: {1}", moduleId, strC);
                    }
                    timeC = (timeC / 2) - (timeC / 2) % 1;
                    GenerateStrings(timeC, strC, 3);
                    Text.text = strC;
                }
            }

            if (startTime - Mathf.Floor(Bomb.GetTime()) > 1)
            {
                active = true;
            }

            if (timeA == timeB && active == true && stages == 0 && moduleReady == true)
            {
                stages = 1;
                Text.text = strB;
                Lights[0].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 1 automatically complete due to time becoming the same as Stage 2. {1} {2} {3}", moduleId, strA, strB, strC);
                
            }
            if (timeB == timeC && active == true && stages == 1 && moduleReady == true)
            {
                stages = 2;
                Text.text = strC;
                Lights[1].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 2 automatically complete due to time becoming the same as Stage 3.", moduleId, strA, strB, strC);
            }
            if (timeC < 5 && active == true && stages == 2 && moduleReady == true)
            {
                GetComponent<KMBombModule>().HandlePass();
                moduleSolved = true;
                stages = 3;
                Text.text = "!!!";
                Lights[2].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 3 automatically complete due to time becoming under 5 seconds. Module Solved.", moduleId, strA, strB, strC);
            }
        } else if (zenMode == true)
        {
            if (stages == 0)
            {
                if (Mathf.Floor(Bomb.GetTime()) > timeC)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    timeC = timeC + (timeC / 2 - (timeC / 2) % 1);
                    timeB = timeB + timeC;
                    timeA = timeA + timeC;
                    GenerateStrings(timeC, strC, 3);
                    GenerateStrings(timeB, strB, 2);
                    GenerateStrings(timeA, strA, 1);
                    Text.text = strC;
                    Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 1 time. Strike! New time: {1}", moduleId, strC);
                    Debug.Log("OKAY: " + timeA + " " + timeB + " " + timeC);
                }
            }
            else if (stages == 1)
            {
                if (Mathf.Floor(Bomb.GetTime()) > timeB)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    timeB = timeB + (timeB / 2 - (timeB / 2) % 1);
                    timeA = timeA + timeB;
                    GenerateStrings(timeB, strB, 2);
                    GenerateStrings(timeA, strA, 1);
                    Text.text = strB;
                    Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 2 time. Strike! New time: {1}", moduleId, strB);
                    Debug.Log("OKAY: " + timeA + " " + timeB + " " + timeC);
                }
            }
            else if (stages == 2)
            {
                if (Mathf.Floor(Bomb.GetTime()) > timeA)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    timeA = timeA + (timeA / 2 - (timeA / 2) % 1);
                    GenerateStrings(timeA, strA, 1);
                    Text.text = strA;
                    Debug.LogFormat("[Timing is Everything #{0}] Missed Stage 3 time. Strike! New time: {1}", moduleId, strA);
                    Debug.Log("OKAY: " + timeA + " " + timeB + " " + timeC);
                }
            }

            if (startTime - Mathf.Floor(Bomb.GetTime()) > 1)
            {
                active = true;
            }

            if (timeA == timeB && active == true && stages == 0)
            {
                if (zenMode == false) { 
                stages = 1;
                Text.text = strB;
                Lights[0].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 1 automatically complete due to time becoming the same as Stage 2. {1} {2} {3}", moduleId, strA, strB, strC);
                }
                else
                {
                    timeA = timeA * 2;
                }
            }
            if (timeB == timeC && active == true && stages == 1)
            {
                stages = 2;
                Text.text = strC;
                if (zenMode == true)
                {
                    Text.text = strA;
                }
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
    }

    void PressButton()
    {
        Button.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (zenMode == false)
        {
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
                if (timeMode == true)
                {
                    Debug.LogFormat("[Timing is Everything #{0}] Submit pressed at the wrong time of '{1}' on Stage {2}. Strike!", moduleId, Bomb.GetFormattedTime(), stages + 1);
                }
                else
                {
                    Debug.LogFormat("[Timing is Everything #{0}] Too early on Stage {1}. Strike!", moduleId, stages + 1);
                }
            }
        } else
        {
            if (stages == 0 && Mathf.Floor(Bomb.GetTime()) == timeC)
            {
                stages = 1;
                Text.text = strB;
                Lights[0].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 1 complete.", moduleId);
            }
            else if (stages == 1 && Mathf.Floor(Bomb.GetTime()) == timeB)
            {
                stages = 2;
                GenerateStrings(timeA, strA, 1);
                Text.text = strA;
                Lights[1].GetComponent<MeshRenderer>().material = GreenMat;
                Debug.LogFormat("[Timing is Everything #{0}] Stage 2 complete.", moduleId);
                Debug.Log(timeA);
            }
            else if (stages == 2 && Mathf.Floor(Bomb.GetTime()) == timeA)
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
                if (timeMode == true)
                {
                    Debug.LogFormat("[Timing is Everything #{0}] Submit pressed at the wrong time of '{1}' on Stage {2}. Strike!", moduleId, Bomb.GetFormattedTime(), stages + 1);
                }
                else
                {
                    Debug.LogFormat("[Timing is Everything #{0}] Too early on Stage {1}. Strike!", moduleId, stages + 1);
                }
            }
        }
    }

    private IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        if (TimeModeActive)
        {
            timeMode = true;
            Debug.LogFormat("[Timing is Everything #{0}] Current Mode: Time", moduleId);
            Debug.LogFormat("[Timing is Everything #{0}] Time Mode is active! This means that new times will NOT be generated and no strikes will occur unless pressed at an incorrect time!", moduleId);
            Button.GetComponent<MeshRenderer>().material = buttonColors[1];
        }
        else if (ZenModeActive)
        {
            zenMode = true;
            Debug.LogFormat("[Timing is Everything #{0}] Current Mode: Zen", moduleId);
            Debug.LogFormat("[Timing is Everything #{0}] Zen Mode is active! This means that times go up instead of down!", moduleId);
            Button.GetComponent<MeshRenderer>().material = buttonColors[2];
            timeA *= 2;
            //timeB += 30;
            //timeC += 30;
            GenerateStrings(timeA, strA, 1);
            //GenerateStrings(timeB, strB, 2);
            //GenerateStrings(timeC, strC, 3);
            Text.text = strC;
        } else if (SteadyModeActive)
        {
            steadyMode = true;
            Debug.LogFormat("[Timing is Everything #{0}] Current Mode: Steady", moduleId);
            Debug.LogFormat("[Timing is Everything #{0}] Steady Mode is active! This means... just be careful I haven't dealt with this yet, sorry!", moduleId);
            Button.GetComponent<MeshRenderer>().material = buttonColors[3];
        }
        else
        {
            Debug.LogFormat("[Timing is Everything #{0}] Current Mode: Normal", moduleId);
        }
        moduleReady = true;
        Debug.LogFormat("[Timing is Everything #{0}] The times are: {1}, {2}, and {3}.", moduleId, strA, strB, strC);
        StopCoroutine("delay");
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
                        string tem = "";
                        if(temp < 10)
                            tem = "0" + temp;
                        else
                            tem = "" + temp;
                        tem += parameters[1].Substring(4, 3);
                        parameters[1] = tem;
                    }

                    Debug.LogFormat("[TiE #{0}] {1}", moduleId, parameters[1]);
                    //yield return "sendtochat Submit time set for '" + parameters[1] + "'";
                    while (true)
                    {
                        String time = Bomb.GetFormattedTime();
                        int millisecondindex = time.IndexOf('.');
                        if (millisecondindex != -1)
                        {
                            time = "00:"+time.Substring(0, millisecondindex);
                        }
                        if (!time.Equals(parameters[1])) yield return "trycancel The submit button's press was cancelled due to a cancel request.";
                        else break;
                    }
                    Button.OnInteract();
                }
            }
            yield break;
        }
    }
}
