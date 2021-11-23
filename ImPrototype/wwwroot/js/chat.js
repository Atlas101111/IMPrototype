"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/ImPrototype/chatHub").build();
var userId = "";
//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveBatchMessages", function (ChatMessages, MaxMessageId, RequestId) {
    try {
        var localMessages = JSON.parse(localStorage.getItem("Mailbox"));
        var localCacheMessages = JSON.parse(localStorage.getItem("Cache"));

        if (localMessages == null) {
            localMessages = {
                "ExpectMessageId": 0,
                "AccountUuid": userId,
                "Messages": []
            };
        }
        if (MaxMessageId < localMessages["ExpectMessageId"]) {
            // A return with messages already been saved locally
            return;
        }
        
        if (localCacheMessages != null && localCacheMessages.length > 0 && RequestId != null) {
            for (let cached of localCacheMessages) {
                localCacheMessages.shift();
                if (cached["RequestId"] == RequestId) {
                    ChatMessages.push(cached["Message"])
                    localMessages["ExpectMessageId"] = cached["Message"]["messageId"] + 1;
                    localStorage.setItem("Cache", JSON.stringify(localCacheMessages));
                    break;
                }
            }
        }

        for (let message of ChatMessages) {
            var li = document.createElement("li");
            document.getElementById("messagesList").appendChild(li);
            li.textContent = `${message["from"]} says ${message["content"]}`;
            localMessages["Messages"].push(message);
            localMessages["ExpectMessageId"] = message["messageId"] + 1;
        }
        connection.invoke("ACK", { "AccountUuid": userId, "ExpectMessageId": localMessages["ExpectMessageId"] })
            .catch(function (err) {
                return console.error(err.toString())
            });
        localStorage.setItem("Mailbox", JSON.stringify(localMessages));
    }
    catch (e) {
        console.error(e);
    }
    
});

connection.on("ReceiveMessage", function (chatMessage) {
    var localMessages = JSON.parse(localStorage.getItem("Mailbox"));
    if (localMessages == null) {
        console.error("No localmessages");
    }
    if (chatMessage["messageId"] != localMessages["ExpectMessageId"]) {
        // In-continuous, save to cache and pull
        // Only need to keep the message with largest MessageId
        var localCache = JSON.parse(localStorage.getItem("Cache"));
        var requestId = "RQ" + "127.0.0.1" + (new Date()).valueOf();
        if (localCache == null) {
            localCache = [];
        }
        for (let message of localCache) {
            if (message["Message"]["messageId"] > chatMessage["messageId"]) {
                return;
            }
        }
        localCache.push({ "Message": chatMessage, "RequestId": requestId });
        localStorage.setItem("Cache", JSON.stringify(localCache));
        var size = chatMessage["messageId"] - localMessages["ExpectMessageId"];
        connection.invoke("PullOffline",
            {
                "AccountUuid": userId,
                "Offset": localMessages["ExpectMessageId"],
                "Size": chatMessage["messageId"] - localMessages["ExpectMessageId"],
                "RequestId": requestId
            });
    }
    else {
        // continuous, save and show
        var li = document.createElement("li");
        document.getElementById("messagesList").appendChild(li);
        li.textContent = `${chatMessage["from"]} says ${chatMessage["content"]}`;
        localMessages["ExpectMessageId"] += 1;
        localMessages["Messages"].push(chatMessage);
        connection.invoke("ACK",
            {
                "AccountUuid": userId,
                "ExpectMessageId": localMessages["ExpectMessageId"]
            }
        );
        localStorage.setItem("Mailbox", JSON.stringify(localMessages));
    }

});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var toUser = document.getElementById("destUserInput").value;
    var uniqueKey = "127.0.0.1" + (new Date()).valueOf();
    connection.invoke("SendMessageP2P", user, toUser, message, uniqueKey).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("Login").addEventListener("click", function (event) {
    userId = document.getElementById("userInput").value;
    connection.invoke("Login", userId).catch(function (err) {
        return console.error(err.toString());
    });
    var localMessages = JSON.parse(localStorage.getItem("Mailbox"));
    var offset = 0;
    if (localMessages != null) {
        offset = localMessages["ExpectMessageId"];
    }
    connection.invoke("PullOffline", { "AccountUuid": userId, "Offset": offset, "Size": 10 }).catch({
        function(err) {
            return console.error(err.toString());
        }
    });
    event.preventDefault();
});