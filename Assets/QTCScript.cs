using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QTCScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Renderer[] segments;
    public Material[] states;
    public TextMesh display;

    private readonly string[] dispchars = new string[5] { "0", "+", "-", "\u00b1", "\u2213" };
    private readonly bool[][] digits = new bool[10][]
    {
        new bool[7]{ true, true, true, false, true, true, true},
        new bool[7]{ false, false, true, false, false, true, false},
        new bool[7]{ true, false, true, true, true, false, true},
        new bool[7]{ true, false, true, true, false, true, true},
        new bool[7]{ false, true, true, true, false, true, false},
        new bool[7]{ true, true, false, true, false, true, true},
        new bool[7]{ true, true, false, true, true, true, true},
        new bool[7]{ true, false, true, false, false, true, false},
        new bool[7]{ true, true, true, true, true, true, true},
        new bool[7]{ true, true, true, true, false, true, true}
    };
    private string[] ans = new string[2];
    private int[] ansints = new int[2];
    private string sub = "";
    private bool collapse;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    switch (b)
                    {
                        case 5:
                            button.AddInteractionPunch();
                            Audio.PlaySoundAtTransform("Measure", transform);
                            if (!collapse)
                            {
                                collapse = true;
                                int r = Random.Range(0, 2);
                                QuantumdaleDingle(ansints[r].ToString(), ansints[r].ToString());
                            }
                            break;
                        case 6:
                            button.AddInteractionPunch();
                            sub = "";
                            display.text = "";
                            break;
                        case 7:
                            button.AddInteractionPunch();
                            if (sub.Length > 9)
                            {
                                Debug.LogFormat("[Quantum Ternary Converter #{0}] Submitted {1}. {2}", moduleID, display.text, ans.Contains(sub) ? "Correct." : "Incorrect. Generating new number.");
                                if (ans.Contains(sub))
                                {
                                    Audio.PlaySoundAtTransform("Solve", transform);
                                    moduleSolved = true;
                                    StartCoroutine("Solve");
                                }
                                else
                                {
                                    module.HandleStrike();
                                    sub = "";
                                    display.text = "";
                                    Generate();
                                }
                            }
                            break;
                        default:
                            button.AddInteractionPunch(0.3f);
                            if (sub.Length < 12)
                            {
                                sub += b.ToString();
                                display.text += dispchars[b];
                            }
                            break;
                    }
                }
                return false;
            };
        }
        Generate();
    }

    private int[] Dec(string x)
    {
        int[] z = new int[2];
        for(int i = 0; i < x.Length; i++)
        {
            z[0] *= 3; z[1] *= 3;
            switch (x[i])
            {
                case '1': z[0]++; z[1]++; break;
                case '2': z[0]--; z[1]--; break;
                case '3': z[0]++; z[1]--; break;
                case '4': z[0]--; z[1]++; break;
            }
        }
        return z;
    }

    private void QuantumdaleDingle(string a, string b)
    {
        if (a[0] == '-' && b[0] == '-') segments[42].material = states[2];
        else if (a[0] == '-' || b[0] == '-') segments[42].material = states[1];
        else segments[42].material = states[0];
        a = new string(a.Reverse().ToArray()).Replace("-", "");
        b = new string(b.Reverse().ToArray()).Replace("-", "");
        bool[][] t = new bool[2][]{new bool[7], new bool[7]};
        for (int i = 0; i < 6; i++)
        {
            t[0] = i >= a.Length ? new bool[7] : digits[a[i] - '0'];
            t[1] = i >= b.Length ? new bool[7] : digits[b[i] - '0'];
            for(int j = 0; j < 7; j++)
            {
                if (t[0][j] && t[1][j]) segments[(i * 7) + j].material = states[2];
                else if (t[0][j] || t[1][j]) segments[(i * 7) + j].material = states[1];
                else segments[(i * 7) + j].material = states[0];
            }
        }
    }

    private void Generate()
    {
        ans[0] = "";
        collapse = false;
        int l = Random.Range(8, 11);
        for (int i = 0; i < l; i++)
        {
            int r = Random.Range(0, 5);
            ans[0] += r.ToString();
        }
        int d = Random.Range(0, ans[0].Count());
        ans[0] = ans[0].Substring(0, d) + "3" + ans[0].Substring(d);
        d = Random.Range(1, 5);
        ans[0] = d.ToString() + ans[0];
        ans[1] = ans[0].Replace("3", "5").Replace("4", "3").Replace("5", "4");
        ansints = Dec(ans[0]);
        Debug.LogFormat("[Quantum Ternary Converter #{0}] The display shows the superposition of the numbers: {1} and {2}.", moduleID, ansints[0], ansints[1]);
        Debug.LogFormat("[Quantum Ternary Converter #{0}] Coverting to balanced ternary yields: {1} and {2}.", moduleID, string.Join("", ans[0].Select(x => "0+-+-"[x - '0'].ToString()).ToArray()), string.Join("", ans[1].Select(x => "0+-+-"[x - '0'].ToString()).ToArray()));
        Debug.LogFormat("[Quantum Ternary Converter #{0}] Enter {1}.", moduleID, string.Join("", ans[0].Select(x => dispchars[x - '0']).ToArray()));
        QuantumdaleDingle(ansints[0].ToString(), ansints[1].ToString());
    }

    private IEnumerator Solve()
    {
        for (int i = 0; i < 20; i++)
        {
            foreach (Renderer s in segments)
                s.material = states[Random.Range(0, 3)];
            yield return new WaitForSeconds(0.05f);
        }
        foreach (Renderer s in segments)
            s.material = states[0];
        display.text = "";
        module.HandlePass();
    }
}
