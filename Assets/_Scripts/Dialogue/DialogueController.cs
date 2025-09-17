using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private floart typeSpeed = 10;

    private Queue<string> paragraphs = new Queue<string>();
    private bool conversationEnded ;
    private string p;
    private bool isTypeing;
    private Coroutine typingDialogueCoroutine;
    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_SPEED = 0.1f;


    public void DisplayNextParagraph(DialogueText dialogueText){
            if(paragraphs.Count == 0){
                if(!conversationEnded){
                    StartConversation(dialogueText);
                }else if(conversationEnded && !isTypeing){
                     EndConversation()
                     return;
                }
            }
          if(!isTypeing){
              p = paragraphs.Dequeue();
              typingDialogueCoroutine = StartCoroutine(TypeDialogueText(p));

          }
          else{
              finishParagraphEarly();
          }

            if(paragraphs.Count == 0){
                conversationEnded = true;
         }
    }
    private void StartConversation(DialogueText dialogueText){
        if(!gameObject.activeSelf){
            gameObject.SetActive(true);
        }
        NPCNameText.text = dialogueText.speakerName;
        for(int i = 0; i < dialogueText.paragraphs.Length; i++){
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
        }
    }
    private void EndConversation(){
        
        paragraphs.Clear();
        conversationEnded = false;
        if(gameObject.activeSelf){
            gameObject.SetActive(false);
        }

    }
    private IEnumerator TypeDialogueText(string p){
        isTypeing = true;
        NPCDialogueText.text = "";
        string originalText =p;
        string displayedText=""; 
        int alphaIndex = 0;
        foreach(char c in p.ToCharArray()){
            alphaIndex++;
            NPCDialogueText.text = originalText;
            displayedText = NPCDialogueText.text.Insert(alphaIndex,HTML_ALPHA);
            NPCDialogueText.text = displayedText;
            yield return new WaitForSeconds(MAX_TYPE_SPEED/typeSpeed);
        }
        isTypeing = false;
    }
    private  void finishParagraphEarly(){
        StopCoroutine(typingDialogueCoroutine);

        NPCDialogueText.text = p;

        isTypeing = false;
    }
    
}