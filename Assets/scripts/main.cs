using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; //to operate on TMP elements
//using System.Diagnostics;

public class main : MonoBehaviour
{
    public bool android;
    private AudioClip[] instrument;
    private AudioSource[] audio = new AudioSource[50];
    private Button sb,cb,rb,hb,ib,pab;
    private InputField textbox;
    private GameObject info, pa, audioSources;
    private Text timer,code,infotext,log;
    private bool playing = false, merging, info1, toneMod = false, end, cut, altPressed = false;
    private int czas = 0, basicgroup = 3, repeat,cdplay;
    private char newtone;
    private byte sleep,s,nextAudio;
    private float waittime = 0.125f;
    private Text pbtext;
    public string[] v,p;
    public byte[] cd;
    private List<byte> newCD;
    private int[] a, wait, group, tone, sound;
    private float[] ton;
    private bool[] plays;
    private string[] name = new string[18] { "Banjo", "Bass", "BassAttack", "BD", "Bell", "Bit", "CowBell", "Didgeridoo", "Flute", "Guitar", "Harp", "Harp2", "Hat", "IceChime", "IronXylophone", "Pling", "Snare", "Xylobone" };
    private TextEditor te = new TextEditor();

    void Start()
    {
        sb = GameObject.Find("PButton").GetComponent<Button>();
        cb = GameObject.Find("CButton").GetComponent<Button>();
        rb = GameObject.Find("RButton").GetComponent<Button>();
        hb = GameObject.Find("HButton").GetComponent<Button>();
        ib = GameObject.Find("IButton").GetComponent<Button>();
        pa = GameObject.Find("Info/PAButton");
        audioSources = GameObject.Find("/Canvas/AudioSources");
        pab = pa.GetComponent<Button>();
        info = GameObject.Find("Info");
        infotext = GameObject.Find("Info/Text").GetComponent<Text>();
        textbox = GameObject.Find("songcode").GetComponent<InputField>();
        code = GameObject.Find("songcode/code").GetComponent<Text>();
        log = GameObject.Find("log").GetComponent<Text>();
        timer = GameObject.Find("/Canvas/botinterface/timer/Text").GetComponent<Text>();
        info.SetActive(false);
        sb.onClick.AddListener(Stop);
        cb.onClick.AddListener(Compile);
        rb.onClick.AddListener(Restart);
        ib.onClick.AddListener(InstrumentInfo);
        hb.onClick.AddListener(Info);
        pab.onClick.AddListener(PlayAll);
        pbtext = sb.GetComponentInChildren<Text>();
        instrument = new AudioClip[name.Length];
        for (int i = 0; i < name.Length; i++) instrument[i] = Resources.Load<AudioClip>("sounds/" + name[i]);
        for (int i = 0; i < 50; i++) audio[i] = audioSources.AddComponent<AudioSource>();
        float b = basicgroup/2;
        int splitGroups=basicgroup+1;
        ton = new float[12 * splitGroups + 1];
        for(int i = 0; i < 12; i++)
        {
            float z = Mathf.Pow(2,(i / 12f));
            for(int j = 0; j < splitGroups; j++)
            {
                ton[j * 12 + i] = z * Mathf.Pow(2, j - b);
            }
        }
        ton[12 * splitGroups] = Mathf.Pow(2, splitGroups - b);
        if (android)
        {
            GameObject.Find("songcode").GetComponent<InputField>().enabled = false;
            GameObject.Find("songcode/Placeholder").SetActive(false);
            log.text = "Click Load to paste your code";
        }
        GameObject.Find("songcode").GetComponent<InputField>().text="( Fur Elise )\nP0{k2j2k2j2k2f2i2g2d4 >ae<d f4}\nV0{G2S10 P0 >eil<g4>e< P0 >eml<d4}";
        Disable(sb, true);
        Disable(rb, true);
    }
    IEnumerator Repet()
    {
        while (true)
        {
            yield return new WaitForSeconds(waittime);
            czas++;
            timer.text = "" + czas;
            sleep--;
            if (sleep == 0) PlaySound();
        }
    }
    void PlaySound()
    {
        if (end)
        {
            playing = false;
            StopCoroutine("Repet");
            Disable(sb, true);
            log.text = "Playing completed";
            return;
        }
        cdplay++;
        audio[nextAudio].clip = instrument[cd[cdplay]];
        cdplay++;
        audio[nextAudio].pitch = ton[cd[cdplay]];
        audio[nextAudio].PlayScheduled(AudioSettings.dspTime + 1.25f);
        nextAudio++;
        if (nextAudio == 50) nextAudio = 0;
        cdplay++;
        if (cd[cdplay] == 0)
        {
            PlaySound();
            return;
        }
        else if (cd[cdplay] == 255)
        {
            end = true;
            sleep = 10;
        }
        else sleep = cd[cdplay];
    }
    int Read(int n)
    {
            a[n]++;
            return v[n][a[n]];
    }
    int Check(int n)
    {
            return v[n][a[n]+1];
    }
    int CReadInt(int n) => code.text[n] - 48;
    int ReadInt2(int n) => 10 * (Read(n) - 48) + Read(n) - 48;
    void AddText(int type, int el, string z)
    {
        if (type == 1) v[el] += z;
        else if (type == 2) p[el] += z;
    }
    void AddStep(int n, int t)
    {
        int i = sound[n];
        newCD.Add(sleep);
        newCD.Add((byte)sound[n]);
        newCD.Add((byte)(group[n] * 6 + t + tone[n]));
        sleep = 0;
        if (Check(n)==48){
            Read(n);
            ReadPath(n);
        }else{
            wait[n]=1;
        }
    }
    void ReadPath(int n)
    {
        int z = Read(n);
        if (49 < z && z < 58) wait[n]=z-49;
        else if (96 < z && z < 110) AddStep(n, z - 97);
        else if (z == 125) plays[n] = false;// }
        else if (z == 87) wait[n] = ReadInt2(n);// W
        else
        {
            if (z == 83) sound[n] = ReadInt2(n);// S
            else if (z == 71) group[n] = Read(n) - 48;// G
            else if (z == 60) group[n]++;//<
            else if (z == 62) group[n]--;//>
            else if (z == 84) tone[n] += Read(n) - 48;// T
            else if (z == 86)// V
            {
                z = Read(n) - 48;
                plays[z] = true;
                a[z] = -1;
                group[z] = basicgroup;
                ReadPath(z);
            }
            else if (z == 88)// X
            {
                if (cut) merging = false;
                else
                {
                    newCD = new List<byte>();
                    sleep = 1;
                    cut = true;
                }
            }
            ReadPath(n);
        }
    }
    void Stop()
    {
        playing = false;
        StopCoroutine("Repet");
        Disable(sb, true);
        log.text = "Playing stopped";
        for (int i = 0; i < audio.Length; i++) audio[i].Stop();
    }
    void Compile()
    {
        log.text = "Compiling...";
        waittime = 0.125f;
        if (android)
        {
            te.text = "";
            te.Paste();
            code.text = te.text;
        }
        v = new string[10];
        p = new string[10];
        int type = 0,el=0,i = 0,n = code.text.Length;
        char z;
        while(i<n)
        {
            z = code.text[i];
            while (z == ' ')
            {
                i++;
                z = code.text[i];
            }
            if (z == '(')
            {
                while (z != ')')
                {
                    i++;
                    z = code.text[i];
                }
                i++;
                z = code.text[i];
                while (z == ' ')
                {
                    i++;
                    z = code.text[i];
                }
            }
            if (type == 0)
            {
                if (z == 'T')
                {
                    waittime = 0.001f * int.Parse(code.text.Substring(i+1, 3));
                    i += 3;
                }
                else if (z == 'V')
                {
                    type = 1;
                    el = System.Convert.ToInt32(code.text[i+1]) - 48;
                    i += 2;
                }
                else if (z == 'P')
                {
                    type = 2;
                    el = System.Convert.ToInt32(code.text[i+1]) - 48;
                    i += 2;
                }
            }
            else
            {
                if (z == 'T')
                {
                    i++;
                    if (code.text[i] != '-') newtone = code.text[i];
                    else
                    {
                        i++;
                        newtone = System.Convert.ToChar(96 - code.text[i]);
                    }
                    AddText(type, el, "T" + newtone);
                    newtone = System.Convert.ToChar(96 - newtone);
                    toneMod = true;
                }
                else if (z == 'P')
                {
                    i++;
                    int b = CReadInt(i);
                    AddText(type, el, p[b]);
                    if (toneMod)
                    {
                        toneMod = false;
                        AddText(type, el, "T" + newtone);
                    }
                }
                else if (z == 'R')
                {
                    repeat = 10 * CReadInt(i + 1) + CReadInt(i + 2);
                    i += 4;
                    int b = CReadInt(i);
                    for (int a = 0; a < repeat; a++) AddText(type, el, p[b]);
                    if (toneMod)
                    {
                        toneMod = false;
                        AddText(type, el, "T" + newtone);
                    }
                }
                else if (z == '}')
                {
                    if (type == 1) v[el] += z;
                    type = 0;
                }
                else AddText(type, el, "" + z);
            }
            i++;
        }
        Merge();
        Restart();
    }
    void Merge()
    {
        newCD = new List<byte>();
        plays = new bool[10];
        wait = new int[10];
        a = new int[10];
        sound = new int[10];
        tone = new int[10];
        group = new int[10];
        plays[0] = true;
        cut = false;
        merging = true;
        sleep = 0;
        a[0] = -1;
        group[0] = basicgroup;
        while (merging)
        {
            sleep ++;
            bool any = false;
            bool[] plays2 = (bool[])plays.Clone();
            for (int i = 0; i < 10; i++) if (plays2[i])
                {
                    any = true;
                    if (wait[i] == 0) ReadPath(i);
                    wait[i]--;
                }
            if (any == false) break;
        }
        newCD.Add((byte)255);
        cd = newCD.ToArray();
    }
    void Restart()
    {
        if (playing) Stop();
        log.text = "Preparing...";
        sleep = cd[0];
        cdplay = 0;
        czas = 0;
        nextAudio = 0;
        end = false;
        timer.text = "0";
        playing = true;
        StartCoroutine("Repet");
        log.text = "Playing";
        Disable(rb, false);
        Disable(sb, false);
    }
    void InstrumentInfo()
    {
        if (info.activeSelf && info1)
        {
            info.SetActive(false);
            Pressed(0, false);
        }
        else
        {
            Pressed(0, true);
            Pressed(1, false);
            info.SetActive(true);
            pa.SetActive(true);
            info1 = true;
            infotext.text = "";
            for(int i = 0; i < name.Length; i++)
            {
                infotext.text += "S";
                if (i < 10) infotext.text += "0";
                infotext.text += i + " " + name[i] + '\n';
            }
        }
    }
    void Info()
    {
        if (info.activeSelf && !info1)
        {
            info.SetActive(false);
            Pressed(1, false);
        }
        else
        {
            Pressed(0, false);
            Pressed(1, true);
            info.SetActive(true);
            pa.SetActive(false);
            info1 = false;
            infotext.text = "Assigments:\nV0{any code} - creates a path which will be played when something calls it\n";
            infotext.text += "P0{any code} - creates a procedure which will be played when something calls it\n";
            infotext.text += "Comments:\nSpaces and code inside round brackets will be ignored\n";
            infotext.text += "Commands:\n(Commands are inside of a procedures or paths)\n";
            infotext.text += "V1 - calls a path. V0 is called automatically when the program is run\n";
            infotext.text += "P1 - calls a procedure. Further code will be executed when the procedure is done. Procedure must be declared before calling\n";
            infotext.text += "a-m  - plays a tone and waits 1 time unit, unless 0 is declared next (to avoid waiting)\nthere is 13 different tones with pitch between 1 and 2, from a to m\n";
            infotext.text += "G4 - changes tone group in the path (from G0 to G6). Each group has 2^0.5 times greater pitch than previuos one, G3 is default group\n";
            infotext.text += "< - switches to higher tone group ( tone g becomes tone a, tone m becomes tone g\n";
            infotext.text += "> - switches to lower tone group ( tone g becomes tone m, tone a becomes tone g\n";
            infotext.text += "2-9 - waits given number - 1 of time. \n";
            infotext.text += "W20 - waits given two-digit number of time. \n";
            infotext.text += "S04 - changes instrument for the one with the given index, in the path\n";
            infotext.text += "R05 P2 - repeats the given procedure 5 times\n";
            infotext.text += "T5 P1, T-5 P1, T4 R05 P2 - plays the given procedure modified by a given number of tones\n";
            infotext.text += "X any code X - start playing from the moment when first X occurs and end when second X occurs\n";
            infotext.text += "Other:\n(Other stuff, which are outside procedures and paths)\n";
            infotext.text += "T080 - set new playing speed in milliseconds, by default it is 125ms";
        }
    }
    void PlayAll()
    {
        waittime = 0.125f;
        v = new string[10];
        for (int i = 0; i < name.Length; i++)
        {
            v[0] += "S";
            if (i < 10) v[0] += "0";
            v[0] += i + "a7";
        }
        v[0] += "}";
        Merge();
        Restart();
        log.text = "Playing all sounds";
    }
    void Pressed(int buttonid,bool pressed)
    {
        var myrgb = ib.colors;
        if (pressed)
        {
            myrgb.selectedColor = new Color(0.15f, 0.15f, 0.15f);
            myrgb.normalColor = new Color(0.15f, 0.15f, 0.15f);
        }
        else
        {
            myrgb.selectedColor = new Color(0.25f, 0.25f, 0.25f);
            myrgb.normalColor = new Color(0.25f, 0.25f, 0.25f);
        }
        if (buttonid == 0) ib.colors = myrgb;
        else hb.colors = myrgb;
    }
    void Disable(Button button, bool yes)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        button.interactable = !yes;
        var myrgb = buttonText.color;
        if (yes) myrgb.a = 0.5f;
        else myrgb.a = 1f;
        buttonText.color = myrgb;
    }
    void Update()
    {
        if (altPressed)
        {
            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt)) altPressed = false;
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.C)) Compile();
                if (Input.GetKeyDown(KeyCode.S)&&sb.IsInteractable()) Stop();
                if (Input.GetKeyDown(KeyCode.R)&&rb.IsInteractable()) Restart();
                if (Input.GetKeyDown(KeyCode.I)) InstrumentInfo();
                if (Input.GetKeyDown(KeyCode.H)) Info();
                if (Input.GetKeyDown(KeyCode.A)) PlayAll();
            }
        }
        else if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                textbox.DeactivateInputField();
                altPressed = true;
            }
        }
    }
}
