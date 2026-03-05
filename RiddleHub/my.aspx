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
        body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }
        #riddlesContainer { max-width: 800px; margin: 50px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }
        .topActions { display: flex; gap: 10px; justify-content: flex-end; margin-bottom: 16px; }
        h1 { text-align: center; color: #333; }
        .riddle { margin-bottom: 30px; list-style: none; }
        h3 { margin-bottom: 10px; }
        p { margin-bottom: 5px; }
        .editBtn, .deleteBtn, .accountBtn { padding: 8px 16px; border: none; border-radius: 4px; color: #fff; font-size: 16px; cursor: pointer; margin-right: 8px; }
        .editBtn, .accountBtn { background-color: #007bff; }
        .deleteBtn { background-color: #d9534f; }
        .editBtn:hover, .accountBtn:hover { background-color: #0056b3; }
        .deleteBtn:hover { background-color: #b52b27; }
    </style>
</head>

<body>
    <div id="sessionBridge" style="display:none" data-loggedin="true" data-username="<%= Session["username"] ?? "" %>"></div>
    <div id="usernametxt" style="display: none"><asp:Literal ID="username" runat="server"></asp:Literal></div>
    <div id="riddlesContainer" class="container">
        <div class="topActions" id="account">
            <button class="accountBtn" id="manageBtn">Account Manage</button>
            <button class="deleteBtn" id="signoutBtn">Sign Out</button>
        </div>
        <h1>My Riddles</h1>
        <h2 id="emptyMessage">Nothing in here yet</h2>
        <p id="createMessage">You haven't created any riddles, but you can go <a href="create.aspx">here</a> to create your first one!</p>
        <ul id="riddleList"></ul>
        <div id="loading" style="display: none;"><img src="loading.gif" alt="Loading..." /></div>
    </div>
    <script>
        relayMessage("my");
        document.getElementById('manageBtn').addEventListener('click', () => { window.location.href = '/account.aspx'; });
        document.getElementById('signoutBtn').addEventListener('click', () => { window.location.href = '/signout.aspx'; });

        function showLoading() { document.getElementById('loading').style.display = 'block'; document.getElementById('emptyMessage').style.display = 'none'; document.getElementById('createMessage').style.display = 'none'; }
        function hideLoading() { document.getElementById('loading').style.display = 'none'; }

        async function deleteRiddle(id, listItem) {
            const response = await fetch('/editriddle.aspx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: 'action=delete&id=' + encodeURIComponent(id)
            });
            if (response.ok) {
                listItem.remove();
            }
            if (document.getElementById('riddleList').children.length === 0) {
                document.getElementById('emptyMessage').style.display = 'block';
                document.getElementById('createMessage').style.display = 'block';
            }
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

            if (hint) { const riddleHint = document.createElement('p'); riddleHint.textContent = `Hint: ${hint}`; riddleItem.appendChild(riddleHint); }
            const riddleAnswer = document.createElement('p');
            riddleAnswer.textContent = `Answer: ${answer}`;
            riddleItem.appendChild(riddleAnswer);

            const editButton = document.createElement('button');
            editButton.className = 'editBtn';
            editButton.textContent = 'Edit';
            editButton.addEventListener('click', () => { window.location.href = `/editriddle?id=${id}`; });
            riddleItem.appendChild(editButton);

            const deleteButton = document.createElement('button');
            deleteButton.className = 'deleteBtn';
            deleteButton.textContent = 'Delete';
            deleteButton.addEventListener('click', () => deleteRiddle(id, riddleItem));
            riddleItem.appendChild(deleteButton);

            riddleList.appendChild(riddleItem);
            if (riddleList.children.length > 0) { emptyMessage.style.display = 'none'; createMessage.style.display = 'none'; }
        }

        async function loadRiddles() {
            showLoading();
            const req = await fetch('/myriddles');
            if (req.status === 401) {
                window.location.assign('/login.aspx?return=my');
                return;
            }
            const json = await req.json();
            let found = true;
            try { var riddleArr = json.userRiddles.toReversed(); } catch { found = false; }
            hideLoading();
            if (found) { for (let i = 0; i < riddleArr.length; i++) { addRiddle(riddleArr[i].riddle_text, riddleArr[i].riddle_hint, riddleArr[i].answer, riddleArr[i].riddle_id); } }
            if (document.getElementById('riddleList').children.length === 0) { emptyMessage.style.display = 'block'; createMessage.style.display = 'block'; }
        }
        loadRiddles();
    </script>
</body>

</html>
