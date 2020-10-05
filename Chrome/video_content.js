var video = document.getElementsByTagName("video")[0];

if (video !== undefined)
{
	video.onplaying = function()
	{
		// Play pressed
		chrome.runtime.sendMessage({data: "videoEvent", event: 0, currentTime: video.currentTime, duration: video.duration});
	}

	video.onpause = function()
	{
		// Pause pressed
		chrome.runtime.sendMessage({data: "videoEvent", event: 1, currentTime: video.currentTime, duration: video.duration});
	}

	video.ontimeupdate = function()
	{
		// Play update tick
		chrome.runtime.sendMessage({data: "videoEvent", event: 2, currentTime: video.currentTime, duration: video.duration});
	}

	window.onunload = function()
	{
		// Video frame closed
		chrome.runtime.sendMessage({data: "videoEvent", event: 3, currentTime: video.currentTime, duration: video.duration});
	}
}