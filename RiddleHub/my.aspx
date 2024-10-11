<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="my.aspx.cs" Inherits="RiddleHub.My" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js"></script>

    <head runat="server">
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Document</title>
        <link rel="stylesheet" href="pagestyles.css" />

        <style>
            body {
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }

            #riddlesContainer {
                max-width: 800px;
                margin: 50px auto;
                padding: 20px;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }

            h1 {
                text-align: center;
                color: #333;
            }

            .riddle {
                margin-bottom: 30px;
                list-style: none;
            }

            h3 {
                margin-bottom: 10px;
            }

            p {
                margin-bottom: 5px;
            }

            .editBtn {
                padding: 8px 16px;
                border: none;
                border-radius: 4px;
                background-color: #007bff;
                color: #fff;
                font-size: 16px;
                cursor: pointer;
            }

            .editBtn:hover {
                background-color: #0056b3;
            }
        </style>
    </head>

    <body>
        <div id="usernametxt" style="display: none">
            <asp:Literal ID="username" runat="server"></asp:Literal>
        </div>
        <div id="riddlesContainer" class="container">
            <h1>My Riddles</h1>
            <h2 id="emptyMessage">Nothing in here yet</h2>
            <p id="createMessage">You haven't created any riddles, but you can go <a href="create.aspx">here</a> to
                create your first one!</p>
            <ul id="riddleList"></ul>

            <!-- Loading GIF -->
            <div id="loading" style="display: none;">
                <img src="loading.gif" alt="Loading..." />
            </div>
        </div>
        <script>
            relayMessage("my");

            function showLoading() {
                document.getElementById('loading').style.display = 'block';
                document.getElementById('emptyMessage').style.display = 'none';
                document.getElementById('createMessage').style.display = 'none';
            }

            function hideLoading() {
                document.getElementById('loading').style.display = 'none';
            }

            function addRiddle(question, hint, answer, id) {
                const riddleList = document.getElementById('riddleList');
                const emptyMessage = document.getElementById('emptyMessage');
                const createMessage = document.getElementById('createMessage');

                const riddleItem = document.createElement('li');
                riddleItem.className = 'riddle';

                const riddleQuestion = document.createElement('h3');
                riddleQuestion.textContent = `Riddle: ${question}`;
                riddleItem.appendChild(riddleQuestion);

                if (hint) {
                    const riddleHint = document.createElement('p');
                    riddleHint.textContent = `Hint: ${hint}`;
                    riddleItem.appendChild(riddleHint);
                }

                const riddleAnswer = document.createElement('p');
                riddleAnswer.textContent = `Answer: ${answer}`;
                riddleItem.appendChild(riddleAnswer);

                const editButton = document.createElement('button');
                editButton.className = 'editBtn';
                editButton.textContent = 'Edit';
                editButton.id = id;
                riddleItem.appendChild(editButton);
                editButton.addEventListener('click', () => {
                    window.location.href = `/editriddle?id=${id}`;
                });
                riddleList.appendChild(riddleItem);

                // Check if there are any riddles and update messages
                if (riddleList.children.length > 0) {
                    emptyMessage.style.display = 'none';
                    createMessage.style.display = 'none'; // Hide the create message as well
                }
            }

            // Simulate loading process
            async function loadRiddles() {
                showLoading();



                let req = await fetch("/myriddles?username=" + document.getElementById("usernametxt").textContent.trim())
                let json = await req.json();
                let found = true;
                try {
                    var riddleArr = json.userRiddles.toReversed();

                }
                catch {
                    found = false;
                }
                hideLoading();
                if (found) {

                    for (let i = 0; i < riddleArr.length; i++) {
                        addRiddle(riddleArr[i].riddle_text, riddleArr[i].riddle_hint, riddleArr[i].answer, riddleArr[i].riddle_id)
                    }
                }

                if (document.getElementById('riddleList').children.length === 0) {
                    emptyMessage.style.display = 'block';
                    createMessage.style.display = 'block';
                }

            }

            loadRiddles();
        </script>
    </body>

    </html>