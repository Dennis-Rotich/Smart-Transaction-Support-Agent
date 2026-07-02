window.chatInterop = {
    formatCodeBlocks: function () {
        document.querySelectorAll('pre code').forEach((block) => {
            if (block.parentElement.parentElement.classList.contains("ai-code-wrapper")) return;

            const pre = block.parentElement;

            let language = "code";
            if (block.className && block.className.includes("language-")) {
                language = block.className.split('-')[1].split(" ")[0];
            }

            const wrapper = document.createElement("div");
            wrapper.className = "ai-code-wrapper";

            const header = document.createElement("div");
            header.className = "ai-code-header";

            const langLabel = document.createElement("span");
            langLabel.innerText = language;

            const copyBtn = document.createElement("button");
            copyBtn.className = "ai-copy-btn";
            copyBtn.innerHTML = "Copy";

            copyBtn.onclick = () => {
                navigator.clipboard.writeText(block.innerText).then(() => {
                    copyBtn.innerHTML = "Copied";
                    setTimeout(() => { copyBtn.innerHTML = "Copy" }, 2000);
                });
            };

            header.appendChild(langLabel);
            header.appendChild(copyBtn);

            pre.parentNode.insertBefore(wrapper, pre);
            wrapper.appendChild(header);
            wrapper.appendChild(pre);
        });
    }
};