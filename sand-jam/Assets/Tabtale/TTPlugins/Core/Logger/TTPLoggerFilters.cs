using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tabtale.TTPlugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TTPLoggerFiltersParameterAttribute : Attribute
    {
        public string Parameter { get; }

        public TTPLoggerFiltersParameterAttribute(string parameter)
        {
            Parameter = parameter;
        }
    }
    
    public abstract class TTPLoggerFiltersBaseFilter
    {
        private List<string> cachedParameters;

        protected abstract string Tag { get; }

        private List<string> Parameters
        {
            get
            {
                if (cachedParameters == null)
                {
                    cachedParameters = GetType()
                        .GetCustomAttributes<TTPLoggerFiltersParameterAttribute>()
                        .Select(attr => attr.Parameter)
                        .ToList();
                }
                return cachedParameters;
            }
        }

        public bool Matches(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var parameter in Parameters)
            {
                if (input.Contains($"{Tag}::{parameter}"))
                    return true;
            }

            return false;
        }
    }

    [TTPLoggerFiltersParameter("MissionStarted:")]
    [TTPLoggerFiltersParameter("MissionComplete:")]
    [TTPLoggerFiltersParameter("MissionAbandoned:")]
    [TTPLoggerFiltersParameter("MissionFailed:")]
    [TTPLoggerFiltersParameter("MiniLevelStarted")]
    [TTPLoggerFiltersParameter("MiniLevelCompleted")]
    [TTPLoggerFiltersParameter("MiniLevelFailed")]
    [TTPLoggerFiltersParameter("TutorialStep:")]
    [TTPLoggerFiltersParameter("LevelUp:")]
    public class TTPLoggerFiltersGameProgression : TTPLoggerFiltersBaseFilter
    {
        protected override string Tag => "TTPGameProgression";
    }
    
}