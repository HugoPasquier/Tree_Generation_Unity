using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Rules : MonoBehaviour
{
    /*symmbols
    F: Move forward,drawing a line
    +,-: turn left/right
    [,]: push/pop current state
    */
    //add random chance to choose rules
    //Ruleset for a preset
    public IDictionary<string, List<(string,float)>> RulesSet = new Dictionary<string, List<(string,float)> >();

    //Contain all Rulesets
    public IDictionary< string, (int, IDictionary<string, List<(string,float)>>) > Presets = new Dictionary< string, (int, IDictionary<string, List<(string,float)>>) >();

    //find the first rule matching the character
    string findMatchingRule(char c,IDictionary<string, List<(string,float)>> rs)
    {
        List<(string,float)> rules= new List<(string,float)>();
        string rule=null;
        float minDistance=0;
        
        //try to find a list of rules matching the character
        if(rs.TryGetValue(c.ToString(), out rules))
        {
            float choice=Random.Range(0f, 1f);
            minDistance= Mathf.Abs(Mathf.Abs(rules[0].Item2) - Mathf.Abs(choice));
            //randomly get a rule in the list
            foreach((string,float) possibility in rules){
                if(Mathf.Abs(Mathf.Abs(possibility.Item2) - Mathf.Abs(choice))<= minDistance){
                    rule=possibility.Item1;
                }
            }
            return rule;
        }
        return null;
        /*
        List<string> rules = new List<string,float>();
        //try to find a rule matching the character
        if(rs.TryGetValue(c.ToString(), out rules))
        {
            print(rules);
            return rules;
        }
        return null;*/
    }

    //apply rules to sentence
    string ApplyRulesToSentence (string sentence, IDictionary<string, List<(string,float)>> rs)
    {
        string newSentence = "";
        string rule = null;
        for (int i = 0; i < sentence.Length; i++)
        {
           char c= sentence[i];
            rule = findMatchingRule(c,rs);
            if (rule !=null)
            {
                newSentence +=rule;
            }
            else
            {
                newSentence += c;
            }

        }
        return newSentence;
    }

    //take an axiom and an iterator to return a sentence
    string ApplyRules(string axiom,int iterator, IDictionary<string, List<(string,float)>> rs)
    {
        
        string sentence=axiom;
        for(int i = 0; i < iterator; i++)
        {
            sentence= ApplyRulesToSentence(sentence, rs);
        }
        return sentence;
    }

    void Start()
    {
        //replace "x" symbol with one rule
        /*RulesSet.Add("X",new List<string> {
            "F+[-F-XF-X][+FF][--XF[+X]][++F-X]"
            });*/
        //---------SAPIN-----------------
        //Sapin rules
        RulesSet.Add("V",new List<(string,float)> {
            ("[+++W][---W]YV",1f)
            });

        RulesSet.Add("W",new List<(string,float)> {
            ("+X[-W]Z",1f)
            });
                        
        RulesSet.Add("X",new List<(string,float)> {
            ("-W[+X]Z",1f)
            });
                        
        RulesSet.Add("Y",new List<(string,float)> {
            ("YZ",1f)
            });

        RulesSet.Add("Z",new List<(string,float)> {
            ("[-FFF][+FFF]F",1f)
            });
        //---------------------------------------
        //Sapin Preset, default axiom = VZFFF
        Presets.Add("sapin", (20, new Dictionary<string, List<(string,float)>>(RulesSet)));
        
        RulesSet.Clear();
        //print(Presets["sapin"].Item2["V"][0].Item1);
        //---------CHENE-----------------
        //Sapin rules
        RulesSet.Add("X", new List<(string,float)> {
            ("[-FX]+FX",1f)
            });
        //---------------------------------------

        //Presets(name,(angle,ruleSet))
        //Chene Preset, default axiom = FX
        Presets.Add("chene", (40, new Dictionary<string, List<(string,float)>>(RulesSet)));

        //ApplyRules(axiom, iterations,Presets[presetName].Item2);
        
        print(ApplyRules("VZFFF", 1,Presets["sapin"].Item2));
    }

}
