﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Unity.Model.Editor.Api
{
	public interface ISlotModelEditor : IInspectorGUI
	{
		IReelSetEditor reelSetEditor { get; set; }
	}
}
