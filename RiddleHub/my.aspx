<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="my.aspx.cs" Inherits="RiddleHub.My" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<script src="iframe.js?v=20260305b"></script>

<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>My Riddles</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <style>
        #myRiddlesContainer {
            max-width: 900px;
            margin-bottom: 0;
        }

        #emptyMessage {
            margin-bottom: 8px;
        }

        #createMessage {
            margin-bottom: 20px;
        }

        .question {
            font-size: 20px;
            margin-bottom: 8px;
        }

    </style>
</head>

<body>
    <div id="sessionBridge" style="display:none" data-loggedin="true" data-username="<%= Session["username"] ?? "" %>"></div>
    <div id="usernametxt" style="display: none"><asp:Literal ID="username" runat="server"></asp:Literal></div>

    <div id="myRiddlesContainer" class="container page-card">
        <h1 class="page-title">My Riddles</h1>
        <p id="riddleSummary" class="page-subtitle center">Loading your riddles...</p>

        <h2 id="emptyMessage" class="center" style="display: none;">Nothing in here yet</h2>
        <p id="createMessage" class="center muted" style="display: none;">You have not created any riddles yet. Start from <a href="create.aspx">Create a Riddle</a>.</p>

        <ul id="riddleList" class="riddle-grid"></ul>

        <div id="loading" class="loading">
            <img src="loading.gif" alt="Loading..." />
        </div>
    </div>

    <script>
        relayMessage("my");

        const riddleListElement = document.getElementById("riddleList");
        const emptyMessageElement = document.getElementById("emptyMessage");
        const createMessageElement = document.getElementById("createMessage");
        const loadingElement = document.getElementById("loading");
        const summaryElement = document.getElementById("riddleSummary");

        function updateSummary() {
            const count = riddleListElement.children.length;
            summaryElement.textContent = count === 1 ? "1 riddle" : `${count} riddles`;
        }

        function updateEmptyState() {
            const isEmpty = riddleListElement.children.length === 0;
            emptyMessageElement.style.display = isEmpty ? "block" : "none";
            createMessageElement.style.display = isEmpty ? "block" : "none";
            updateSummary();
        }

        function showLoading() {
            loadingElement.style.display = "block";
            summaryElement.textContent = "Loading your riddles...";
        }

        function hideLoading() {
            loadingElement.style.display = "none";
        }

        async function deleteRiddle(id, listItem) {
            const response = await fetch("/editriddle.aspx", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: "action=delete&id=" + encodeURIComponent(id)
            });

            if (response.ok) {
                listItem.remove();
                updateEmptyState();
            }
        }

        function addRiddle(question, hint, answer, id, order) {
            const riddleItem = document.createElement("li");
            riddleItem.className = "riddle-card";

            const questionTitle = document.createElement("h3");
            questionTitle.className = "question";
            questionTitle.textContent = question;
            riddleItem.appendChild(questionTitle);

            const metaLine = document.createElement("p");
            metaLine.className = "riddle-meta";
            metaLine.textContent = `Riddle #${order}`;
            riddleItem.appendChild(metaLine);

            if (hint) {
                const hintLine = document.createElement("p");
                hintLine.className = "riddle-meta";
                hintLine.textContent = `Hint: ${hint}`;
                riddleItem.appendChild(hintLine);
            }

            const answerBox = document.createElement("p");
            answerBox.className = "riddle-answer";
            answerBox.textContent = `Answer: ${answer}`;
            riddleItem.appendChild(answerBox);

            const actions = document.createElement("div");
            actions.className = "riddle-actions";

            const editButton = document.createElement("button");
            editButton.className = "btn";
            editButton.type = "button";
            editButton.textContent = "Edit";
            editButton.addEventListener("click", () => {
                window.location.href = `/editriddle.aspx?id=${id}`;
            });
            actions.appendChild(editButton);

            const deleteButton = document.createElement("button");
            deleteButton.className = "btn btn-danger";
            deleteButton.type = "button";
            deleteButton.textContent = "Delete";
            deleteButton.addEventListener("click", () => deleteRiddle(id, riddleItem));
            actions.appendChild(deleteButton);

            riddleItem.appendChild(actions);
            riddleListElement.appendChild(riddleItem);
            updateEmptyState();
        }

        async function loadRiddles() {
            showLoading();
            const req = await fetch("/myriddles");

            if (req.status === 401) {
                window.location.assign("/login.aspx?return=my&required=1");
                return;
            }

            const json = await req.json();
            const riddleArr = Array.isArray(json.userRiddles) ? json.userRiddles.slice().reverse() : [];
            hideLoading();

            for (let i = 0; i < riddleArr.length; i++) {
                addRiddle(
                    riddleArr[i].riddle_text,
                    riddleArr[i].riddle_hint,
                    riddleArr[i].answer,
                    riddleArr[i].riddle_id,
                    i + 1
                );
            }

            updateEmptyState();
        }

        loadRiddles();
    </script>
</body>

</html>
