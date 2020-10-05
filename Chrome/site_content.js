chrome.runtime.onMessage.addListener(function (message, sender, sendResponse)
{
	if (message.data === "videoEvent")
	{
		processVideoEvent(message);
	}
});

window.onunload = function()
{
	processVideoEvent({data: "videoEvent", event: 3});
}

var webSocket = new WebSocket('ws://localhost:8080/AnimeDRP');
var animeData;

function processVideoEvent(message)
{
	if (webSocket.readyState === WebSocket.OPEN)
	{
		if (message.event === 0)
		{
			animeData = getAnimeData();
		}
		/*switch(message.event)
		{
			case "playing":
				
				break;
			case "paused":
				webSocket.send('{"action": 1}');
				break;
			case "timeUpdated":
				webSocket.send('{"action": 2,currentTime:' + message.currentTime + '}');
				break;
			case "closed":
				webSocket.send('{"action": 3}');
				break;
			default:
				break;
		}*/
		webSocket.send('{"action":' + message.event + ',"title":"' + animeData.title + '","currentEpisode":' + animeData.currentEpisode + ',"maxEpisodes":' + animeData.maxEpisodes + ',"currentTime":' + message.currentTime + ',"duration":' + message.duration + '}');

		console.log("Video event: " + message.event + " [success]");
	}
	else
	{	
		console.log("Video event: " + message.event + " [failed]");
	}
}

function getAnimeData()
{
	var result = {};
	result.title = document.getElementsByClassName("title")[0].innerHTML;		
	var e = document.getElementById("servers-container").querySelectorAll(".server");

	for (var i = 0; i < e.length; i++) 
	{
		if (e[i].className === 'server')
		{
			result.maxEpisodes = e[i].firstElementChild.childElementCount;
			
			for	(var j = 0; j < result.maxEpisodes; j++)
			{
				if (e[i].firstElementChild.children[j].getElementsByClassName("active").length != 0)
				{
					result.currentEpisode = j + 1;
					break;
				}
			}
			break;
		}
	}
	return result;
}
