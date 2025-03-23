package model;

import java.util.List;

public record SituationSolutionAnswer(Solution plan, RouteOption routeOptions, double quality) {
	public record SolutionAnswerRootObject(List<SituationSolutionAnswer> routes) {}
}
