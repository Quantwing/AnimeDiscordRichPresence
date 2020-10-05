chrome.browserAction.onClicked.addListener(function(tab)
{ 
	// Extension icon click event here
	//alert('icon clicked');
});

chrome.runtime.onMessage.addListener(function (message, sender, sendResponse)
{
	chrome.tabs.query({active: true, currentWindow: true}, function(tabs) 
	{
		chrome.tabs.sendMessage(tabs[0].id, message);
	});
});
