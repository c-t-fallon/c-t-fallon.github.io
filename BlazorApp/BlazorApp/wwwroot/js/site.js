function sendMessageToHost(content) {
    return window?.chrome?.webview?.hostObjects.bridge?.ReceiveMessage(content);
};