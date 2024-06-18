// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AntDesign.Internal.Components
{
    public class RenderControl : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool CanRender { get; set; }

        [Parameter]
        public EventCallback AfterRender { get; set; }

        protected override bool ShouldRender()
        {
            return CanRender;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (AfterRender.HasDelegate)
            {
                AfterRender.InvokeAsync(this);
            }
        }
    }
}
