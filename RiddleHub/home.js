function createRiddle(question, answer, hint) {
  const questionLi = document.createElement("li");
  const questionText = document.createElement("p");

  questionText.textContent = `${question} | `;
  if (hint) {
    const hintLink = document.createElement("a");
    hintLink.href = "#";
    hintLink.textContent = "hint";
    hintLink.onclick = function () {
      alert(hint);
      return false;
    };
    questionText.appendChild(hintLink);
    questionText.appendChild(document.createTextNode(" | "));
  }

  const answerLink = document.createElement("a");
  answerLink.href = "#";
  answerLink.textContent = "answer";
  answerLink.onclick = function () {
    alert(answer);
    return false;
  };

  questionText.appendChild(answerLink);
  questionLi.appendChild(questionText);
  document.getElementById("Riddles").appendChild(questionLi);
}
async function getRiddles() {
    let req = await fetch("/riddles")
    let json = await req.json()
    let riddleList = json.riddleList
    for (let i = 0; i < riddleList.length; i++) {
        if (riddleList[i].riddle_hint !== null && riddleList[i].riddle_hint !== undefined) {
            createRiddle(
                riddleList[i].riddle_text,
                riddleList[i].answer,
                riddleList[i].riddle_hint
            )
        }
        else {
            createRiddle(
                riddleList[i].riddle_text,
                riddleList[i].answer,
            )
        }
    }
}


getRiddles().then(() => {
    createRiddle(
        "What is it that no one wants to have, but no one wants to lose, either?",
        "A lawsuit"
    );

    createRiddle(
        "What has two hands, a round face, always runs, yet always stays in place, too?",
        "A clock",
        "TikTok"
    );
    createRiddle(
        "I sit in the corner while traveling around the world. What am I?",
        "I’m a postage stamp!",
        "Mail"
    );
    createRiddle(
        "How many sides does a circle have?",
        "Two – one inside and one outside!",
        "Perspective"
    );
    createRiddle(
        "What do silk and grass have in common?",
        "They’re both sold by the yard!"
    );
    createRiddle(
        "If you know me, you'll want to share me. If you share me, Ill be gone. What am I?",
        "A secret!"
    );
    createRiddle(
        "I have no hands but can knock on your door, and you must open if I do. What am I?",
        "Opportunity.",
        "Not a human"
    );
})