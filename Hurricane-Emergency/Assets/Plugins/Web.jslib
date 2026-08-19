mergeInto(LibraryManager.library,
{
   CallINITfunction: function() 
  {
    globals.init();
  },

  OnResetDone: function() 
  {
    globals.onResetDone();
  },

  EntityClick: function(id) 
  {
    globals.entityClick(id);
  },

  SendLangCode: function()
	{
		globals.SendLangCode();
	},

  SendEvent: function(name)
	{
		var text = UTF8ToString(name)
		globals.SendEvent(text);
	},

   OnJuneArrives: function(id) 
  {
    globals.OnJuneArrives(id);
  },

  OnMayArrives: function(id) 
  {
    globals.OnMayArrives(id);
  },

  HurricaneWatchOnAnnounced: function(id)
  {
    globals.HurricaneWatchOnAnnounced(id);
  },

    HurricaneWarningOnAnnounced: function(id)
  {
    globals.HurricaneWarningOnAnnounced(id);
  },

  OnInShelter: function(id)
  {
    globals.OnInShelter(id);
  },

  AllClear: function(id)
  {
    globals.AllClear(id);
  },

  GivesReminder: function(id)
  {
    globals.OnGivesReminder(id);
  },
  

});
