using System;
using Xunit;
using FluentAssertions;
using ORA.Tracker;

namespace ORA.Tracker.Tests.Unit
{
    public class ArgumentsTests
    {
        [Fact]
        public void Parse_WhenEmptyArgs_FieldsShouldBeDefault()
        {
            var args = new string[] { };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(Arguments.DefaultPort);
            arguments.ClusterDatabasePath.Should().Be(Arguments.DefaultClusterDatabasePath);
            arguments.IsHelpRequested.Should().Be(false);
        }

        [Fact]
        public void Parse_WhenUnknownOption_ShouldThrowArgumentException()
        {
            var args = new string[] { "--unknown-option" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Unknown option '--unknown-option'."));
        }

        [Fact]
        public void Parse_WhenShortHelpArg_IsHelpRequestedShouldBeTrue()
        {
            var args = new string[] { "-h" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(Arguments.DefaultPort);
            arguments.ClusterDatabasePath.Should().Be(Arguments.DefaultClusterDatabasePath);
            arguments.IsHelpRequested.Should().Be(true);
        }

        [Fact]
        public void Parse_WhenHelpArg_IsHelpRequestedShouldBeTrue()
        {
            var args = new string[] { "--help" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(Arguments.DefaultPort);
            arguments.ClusterDatabasePath.Should().Be(Arguments.DefaultClusterDatabasePath);
            arguments.IsHelpRequested.Should().Be(true);
        }

        [Fact]
        public void Parse_WhenShortPortArgAndMissingParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "-p" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Missing parameter for '-p' option."));
        }

        [Fact]
        public void Parse_WhenPortArgAndMissingParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "--port" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Missing parameter for '--port' option."));
        }

        [Fact]
        public void Parse_WhenShortPortArgAndInvalidParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "-p", "4f5" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Invalid parameter for '-p' option."));
        }

        [Fact]
        public void Parse_WhenPortArgAndInvalidParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "--port", "4f5" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Invalid parameter for '--port' option."));
        }

        [Fact]
        public void Parse_WhenShortPortArg_PortShouldMatch()
        {
            var args = new string[] { "-p", "4555" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(4555);
            arguments.ClusterDatabasePath.Should().Be(Arguments.DefaultClusterDatabasePath);
            arguments.IsHelpRequested.Should().Be(false);
        }

        [Fact]
        public void Parse_WhenPortArg_PortShouldMatch()
        {
            var args = new string[] { "--port", "4555" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(4555);
            arguments.ClusterDatabasePath.Should().Be(Arguments.DefaultClusterDatabasePath);
            arguments.IsHelpRequested.Should().Be(false);
        }

        [Fact]
        public void Parse_WhenShortClusterDatabasePathArgAndMissingParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "-d" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Missing parameter for '-d' option."));
        }

        [Fact]
        public void Parse_WhenClusterDatabasePathArgAndMissingParameter_ShouldThrowArgumentException()
        {
            var args = new string[] { "--database" };

            Action a = () => Arguments.Parse(args);
            a.Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("Missing parameter for '--database' option."));
        }

        [Fact]
        public void Parse_WhenShortClusterDatabasePathArg_ClusterDatabasePathShouldMatch()
        {
            var args = new string[] { "-d", "./new-database-dir" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(Arguments.DefaultPort);
            arguments.ClusterDatabasePath.Should().Be("./new-database-dir");
            arguments.IsHelpRequested.Should().Be(false);
        }

        [Fact]
        public void Parse_WhenClusterDatabasePathArg_ClusterDatabasePathShouldMatch()
        {
            var args = new string[] { "--database", "./new-database-dir" };

            var arguments = Arguments.Parse(args);

            arguments.Should().BeOfType<Arguments>();
            arguments.Port.Should().Be(Arguments.DefaultPort);
            arguments.ClusterDatabasePath.Should().Be("./new-database-dir");
            arguments.IsHelpRequested.Should().Be(false);
        }
    }
}
