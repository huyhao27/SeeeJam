using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Wraith : /* NPC ,*/ Italkable
{
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private DialogueController dialogueController;
    // public override void Interact(){
    //    Talk(dialogueText);
    // }
    public void Talk(DialogueText dialogueText){
        dialogueController.DisplayNextParagraph(dialogueText);
    }
}