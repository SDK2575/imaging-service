	'this script executes at 1 am in order to preload Onbase Users after Image Service pool recycled.
	
	
	Call Main
	
	Sub Main
	On Error Resume Next

   Dim objShell 
   Set objShell = Wscript.CreateObject("Wscript.Shell")
	' create the XMLHTTP object
	Dim LxmlHTTP
	Set LxmlHTTP = CreateObject("Microsoft.XMLHTTP")
	
	Const EVENT_SUCCESS = 0
   Const EVENT_FAILED = 1
   Const EVENT_INFO = 4 	

	Dim sURL
	objShell.LogEvent EVENT_INFO,  "Refresh users started"
	
	sURL = "http://onbiisdwt1/ImagingService/Service.asmx/GetUsersForGroups?PastrUserGroups=MANAGER"
	'Open a request to the web service
	Call LxmlHTTP.open("GET", sURL, false)
	Call LxmlHTTP.setRequestHeader("Content-Type", "application/x-www-form-urlencoded")

	Call LxmlHTTP.send("")

	If LxmlHTTP.Status <> 200 then
		objShell.LogEvent EVENT_FAILED , "Problem with LxmlHTTP send method " & LxmlHTTP.statusText
		Exit sub
	end if
	'Create the XMLDOM object
	Dim LxmlDom
	Set LxmlDom = CreateObject("MSXML.DOMDocument")

	If (LxmlDom Is Nothing) then
	   objShell.LogEvent EVENT_FAILED , "The LxmlDom object failed to initialize."
	   Exit sub
	End If

	'load returned xml string to DOM
	Dim LobjXML
	Set LobjXML = CreateObject("Microsoft.XMLDOM")
	LobjXML.async=false

	LobjXML.loadXML(LxmlHTTP.responseText)
	Set LxmlUserNode = LobjXML.documentElement.childNodes
	LxmlIdNode = LobjXML.documentElement.childNodes.length
	Dim LstrItems
	LstrItems = ""
	for i = 0 to LxmlIdNode -1
		LstrItems = LstrItems & "~" & LxmlUserNode.item(i).childnodes.item(1).text & " (" + LxmlUserNode.item(i).childnodes.item(0).text & ")"
	
	next
   objShell.LogEvent EVENT_SUCCESS , "Users loaded successfully"
End sub







