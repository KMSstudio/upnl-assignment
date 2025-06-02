using System.Collections.Generic;

/// <summary>
/// Provides a queue of structured player inputs.
/// 
/// Author: Serius <tomskang@naver.com>
/// Last Modified: 2025-05-28
/// </summary>

public interface ILocationController {
    PlayerLocation GetLatestLocation();
}
