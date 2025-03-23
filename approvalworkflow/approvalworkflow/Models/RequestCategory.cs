using System;
using approvalworkflow.Enums;
using Humanizer;

namespace approvalworkflow.Models;

public class RequestCategory
{
    public int Id { get; set; }

    public RequestType RequestType { get; set; }
    public int RequiredApproverCount { get; set; }

    public override string ToString()
    {
        return RequestType.Humanize().Titleize();
    }
}
