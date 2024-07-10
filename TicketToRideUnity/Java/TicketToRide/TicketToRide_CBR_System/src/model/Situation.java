package model;

import java.util.List;

import adaptions.AdaptionRequest.AdaptionType;


public record Situation(
    int handTrainCardsCount,
    int destinationCardsCount,
    int availableWagons,
    int minimalAvailableWagons,
    double averageScoreOfRoutes,
    List<RouteOption> routeOptions,
    AdaptionType situationType
) {}
