using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.StateMachine;

/// <summary>
/// State Machine điều phối vòng đời hồ sơ khám sức khỏe
/// </summary>
public static class ExamWorkflowStateMachine
{
    private static readonly Dictionary<RegistrationStatus, HashSet<RegistrationStatus>> AllowedTransitions = new()
    {
        [RegistrationStatus.Pending] = new()
        {
            RegistrationStatus.CheckedIn,
            RegistrationStatus.Cancelled
        },
        [RegistrationStatus.CheckedIn] = new()
        {
            RegistrationStatus.InProgress,
            RegistrationStatus.Cancelled
        },
        [RegistrationStatus.InProgress] = new()
        {
            RegistrationStatus.WaitingForLab,
            RegistrationStatus.WaitingForConclusion,
            RegistrationStatus.Cancelled
        },
        [RegistrationStatus.WaitingForLab] = new()
        {
            RegistrationStatus.WaitingForConclusion,
            RegistrationStatus.InProgress // Nếu chỉ định bổ sung xét nghiệm/chuyên khoa
        },
        [RegistrationStatus.WaitingForConclusion] = new()
        {
            RegistrationStatus.Completed,
            RegistrationStatus.InProgress // Nếu bác sĩ trưởng đoàn yêu cầu khám lại trạm chuyên khoa
        },
        [RegistrationStatus.Completed] = new()
        {
            RegistrationStatus.Delivered,
            RegistrationStatus.WaitingForConclusion // Reopen khi cần hội chẩn / đính chính kết luận
        },
        [RegistrationStatus.Delivered] = new()
        {
            RegistrationStatus.WaitingForConclusion // Reopen trong trường hợp đặc biệt
        },
        [RegistrationStatus.Cancelled] = new()
    };

    public static bool CanTransition(RegistrationStatus currentStatus, RegistrationStatus nextStatus)
    {
        if (currentStatus == nextStatus) return true;
        return AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(nextStatus);
    }

    public static void ValidateTransition(RegistrationStatus currentStatus, RegistrationStatus nextStatus)
    {
        if (!CanTransition(currentStatus, nextStatus))
        {
            throw new InvalidOperationException(
                $"Chuyển đổi trạng thái không hợp lệ: Không thể chuyển từ '{GetStatusName(currentStatus)}' sang '{GetStatusName(nextStatus)}'.");
        }
    }

    /// <summary>
    /// Tự động đánh giá trạng thái tiếp theo dựa trên tiến độ các trạm chuyên khoa và kết quả cận lâm sàng
    /// </summary>
    public static RegistrationStatus CalculateNextStatus(
        bool hasCheckedIn,
        bool allClinicalStationsCompleted,
        bool hasPendingLabOrders,
        bool hasDoctorConclusion)
    {
        if (!hasCheckedIn) return RegistrationStatus.Pending;

        if (hasDoctorConclusion)
        {
            return RegistrationStatus.Completed;
        }

        if (!allClinicalStationsCompleted)
        {
            return RegistrationStatus.InProgress;
        }

        if (hasPendingLabOrders)
        {
            return RegistrationStatus.WaitingForLab;
        }

        return RegistrationStatus.WaitingForConclusion;
    }

    public static string GetStatusName(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Pending => "Chờ khám",
        RegistrationStatus.CheckedIn => "Đã tiếp đón",
        RegistrationStatus.InProgress => "Đang khám",
        RegistrationStatus.WaitingForLab => "Chờ kết quả CLS",
        RegistrationStatus.WaitingForConclusion => "Chờ kết luận",
        RegistrationStatus.Completed => "Hoàn tất",
        RegistrationStatus.Delivered => "Đã trả kết quả",
        RegistrationStatus.Cancelled => "Đã hủy",
        _ => status.ToString()
    };
}

