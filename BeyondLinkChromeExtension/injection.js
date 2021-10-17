function GetStat(header, key)
{
	content = document.body.innerHTML;
	if(header!="")
	{
		if(content.indexOf(header)<0)
		{
			return "";
		}
		else
		{
			content = content.substring(content.indexOf(header)); 
			content = content.substring(header.length);
		}
	}
	if(content.indexOf(key)<0)
	{
		return "";
	}
	else
	{
		content = content.substring(content.indexOf(key));
		content = content.substring(name.length)
		content = content.substring(content.indexOf(">")+1);
		content = content.substring(0,content.indexOf("<"));
		return content;
	}
}

function GetModifier(header)
{
	return GetStat(header, "ddbc-signed-number__sign") + GetStat(header, "ddbc-signed-number__number");
}

function GetAttacks()
{
	var content = document.body.innerHTML;
	var results = [];
	while(content.indexOf("ddbc-combat-attack__name")>-1 && content.indexOf("-name")>-1)
	{
		content = content.substring(content.indexOf("ddbc-combat-attack__name")+24); 
		content = content.substring(content.indexOf("-name")+5);
		content = content.substring(content.indexOf(">")+1);
		var name = content.substring(0,content.indexOf("<"));
		content = content.substring(content.indexOf("<")+1);

		content = content.substring(content.indexOf("ddbc-combat-attack__tohit")+25); 
		content = content.substring(content.indexOf("ddbc-signed-number__sign")+24);
		content = content.substring(content.indexOf(">")+1);
		var sign = content.substring(0,content.indexOf("<"));		
		content = content.substring(content.indexOf("<")+1);
		content = content.substring(content.indexOf("ddbc-signed-number__number")+26);
		content = content.substring(content.indexOf(">")+1);
		var mod = content.substring(0,content.indexOf("<"));		
		content = content.substring(content.indexOf("<")+1);
		content = content.substring(content.indexOf("ddbc-damage")+11);
		content = content.substring(content.indexOf("data-original-title=\"")+21);
		var dmgType = content.substring(0,content.indexOf("\""));		
		content = content.substring(content.indexOf("\"")+1);
		content = content.substring(content.indexOf("ddbc-damage__value")+18);
		content = content.substring(content.indexOf(">")+1);
		var dmgDice = content.substring(0,content.indexOf("<"));		
		content = content.substring(content.indexOf("<")+1);

		results.push('"'+name+'": \"'+sign+mod+'\",\r\n');
		results.push('"'+name+'.damage.type": \"'+dmgType+'\",\r\n');
		results.push('"'+name+'.damage.amount": \"'+dmgDice+'\",\r\n');
	}
	if(results.length>0)
	{
		var last = results.pop();
		last = last.substring(0,last.length-3);
		results.push(last+"\r\n");
	}
	return results;
}

function GetHD()
{
	var used = 0;
	var unused = 0;
	var content = document.body.innerHTML;
	while(content.indexOf("ct-slot-manager__slot ct-slot-manager__slot--used ct-slot-manager__slot--interactive")>-1)
	{
		used = used + 1;
		content = content.substring(content.indexOf("ct-slot-manager__slot ct-slot-manager__slot--used ct-slot-manager__slot--interactive")+84);
	}
	content = document.body.innerHTML;
	while(content.indexOf("ct-slot-manager__slot ct-slot-manager__slot--interactive")>-1)
	{
		unused = unused + 1;
		content = content.substring(content.indexOf("ct-slot-manager__slot ct-slot-manager__slot--interactive")+56);
	}
	var results = [];
	results.push('"Used": '+used+',\r\n');
	results.push('"Total": '+(used+unused)+',\r\n');
	return results;
}

setInterval(()=>
{
	var level = GetStat("","ddbc-character-progression-summary__level"); level = level.substring(level.lastIndexOf(" ")+1);
		
	var specs = 
		  '{"Name": "'+				GetStat("","ddbc-character-name")+'",\r\n'+
		  '"Level": "'+				level+'",\r\n'+
		  '"Race": "'+				GetStat("","ddbc-character-summary__race")+'",\r\n'+
		  '"Class": "'+				GetStat("","ddbc-character-summary__classes")+'",\r\n'+
		  '"AC": '+					GetStat("","ddbc-armor-class-box__value")+',\r\n'+
		  '"HP": {\r\n'+
		  '"Current": '+			GetStat("Current</div><div class=\"ct-health-summary","ct-health-summary__hp-number")+',\r\n'+
		  '"Max": '+				GetStat("Max</div><div class=\"ct-health-summary","ct-health-summary__hp-number")+
		  '},\r\n'+
		  '"HD": {\r\n';
		  
	var hd = GetHD();
	hd.forEach(el => specs = specs + el);
	
	specs = specs + 
		  '},\r\n'+
		  '"Prof": "'+				GetModifier("ct-proficiency-bonus-box__value")+'",\r\n'+
		  '"Move": '+				GetStat("ct-speed-box__box-value","ddbc-distance-number__number")+',\r\n'+
		  '"Init": "'+				GetModifier("ct-combat-tablet__initiative")+'",\r\n'+
		  '"Abilities": {\r\n'+
		  '"STR": "'+				GetModifier(">str</span></div><div class=\"ddbc-ability-summary__primary")+'",\r\n'+
		  '"DEX": "'+				GetModifier(">dex</span></div><div class=\"ddbc-ability-summary__primary")+'",\r\n'+
		  '"CON": "'+				GetModifier(">con</span></div><div class=\"ddbc-ability-summary__primary")+'",\r\n'+
		  '"INT": "'+				GetModifier(">int</span></div><div class=\"ddbc-ability-summary__primary")+'",\r\n'+
		  '"WIS": "'+				GetModifier(">wis</span></div><div class=\"ddbc-ability-summary__primary")+'",\r\n'+
		  '"CHA": "'+				GetModifier(">cha</span></div><div class=\"ddbc-ability-summary__primary")+'"\r\n'+
		  '},\r\n'+
		  '"Saves": {\r\n'+
		  '"STR": "'+				GetModifier(">str</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'",\r\n'+
		  '"DEX": "'+				GetModifier(">dex</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'",\r\n'+
		  '"CON": "'+				GetModifier(">con</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'",\r\n'+
		  '"INT": "'+				GetModifier(">int</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'",\r\n'+
		  '"WIS": "'+				GetModifier(">wis</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'",\r\n'+
		  '"CHA": "'+				GetModifier(">cha</div><div class=\"ddbc-saving-throws-summary__ability-modifier")+'"\r\n'+
		  '},\r\n'+
		  '"Skills": {\r\n'+
		  '"Acrobatics": "'+		GetModifier("Acrobatics</div><div class=\"ct-skills__col--modifier")+'",\r\n'+
		  '"Animal Handling": "'+	GetModifier("Animal Handling</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Arcana": "'+			GetModifier("Arcana</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Athletics": "'+			GetModifier("Athletics</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Deception": "'+			GetModifier("Deception</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"History": "'+			GetModifier("History</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Insight": "'+			GetModifier("Insight</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Intimidation": "'+		GetModifier("Intimidation</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Investigation": "'+		GetModifier("Investigation</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Medicine": "'+			GetModifier("Medicine</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Nature": "'+			GetModifier("Nature</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Perception": "'+		GetModifier("Perception</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Performance": "'+		GetModifier("Performance</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Persuasion": "'+		GetModifier("Persuasion</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Religion": "'+			GetModifier("Religion</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Sleight of Hand": "'+	GetModifier("Sleight of Hand</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Stealth": "'+			GetModifier("Stealth</div><div class=\"ct-skills__col--modifier")+'",\r\n'+		  
		  '"Survival": "'+			GetModifier("Survival</div><div class=\"ct-skills__col--modifier")+'"\r\n'+		  
		  '},\r\n'+
		  '"Attacks": {\r\n';
	
	var attacks = GetAttacks();
	attacks.forEach(el => specs = specs + el);
	
	specs = specs + 
		  '}'+
		  "}";
		  
	// alert(specs);
	
	try
	{
		var connection = new WebSocket('ws://127.0.0.1:9100');
		connection.onopen = function ()
		{
			connection.send(specs);
		};
	}
	catch(e)
	{
		// alert(e);
	}
	
},5000);