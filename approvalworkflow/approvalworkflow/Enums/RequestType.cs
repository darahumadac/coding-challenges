using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace approvalworkflow.Enums;

public enum RequestType
{
    Service,
    [Display(Name = "Role Change")]
    RoleChange
}


