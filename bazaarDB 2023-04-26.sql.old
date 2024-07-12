-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: db
-- Generation Time: Apr 27, 2023 at 04:49 AM
-- Server version: 5.7.42
-- PHP Version: 8.1.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `bazaarDB`
--

-- --------------------------------------------------------

--
-- Table structure for table `Builds`
--

CREATE TABLE `Builds` (
  `buildID` bigint(20) NOT NULL,
  `buildName` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `userID` bigint(20) UNSIGNED NOT NULL,
  `imageID` bigint(20) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `Builds`
--

INSERT INTO `Builds` (`buildID`, `buildName`, `userID`, `imageID`) VALUES
(1, 'build1', 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `Images`
--

CREATE TABLE `Images` (
  `imageID` bigint(20) UNSIGNED NOT NULL,
  `imageName` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `Images`
--

INSERT INTO `Images` (`imageID`, `imageName`) VALUES
(1, '1666815181513802.jpg');

-- --------------------------------------------------------

--
-- Table structure for table `Users`
--

CREATE TABLE `Users` (
  `userID` bigint(20) UNSIGNED NOT NULL,
  `userName` varchar(20) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `email` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `password` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `Users`
--

INSERT INTO `Users` (`userID`, `userName`, `email`, `password`) VALUES
(1, 'test', 'test@gmail.com', '$2a$11$OJGrkLRyeYig1paFFSJKBOXC9qMLAlNH7neAmZAjdU1oXNv4cxI1m'),
(2, 'test2', 'test2@gmail.com', '$2a$11$7.h0W/LCeojTo7qQ8blxQ.ttV.2hEgX5BJTR2v90ITm3k38SPgfHS');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `Builds`
--
ALTER TABLE `Builds`
  ADD PRIMARY KEY (`buildID`),
  ADD KEY `userID` (`userID`),
  ADD KEY `imageID` (`imageID`);

--
-- Indexes for table `Images`
--
ALTER TABLE `Images`
  ADD PRIMARY KEY (`imageID`);

--
-- Indexes for table `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`userID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `Builds`
--
ALTER TABLE `Builds`
  MODIFY `buildID` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `Images`
--
ALTER TABLE `Images`
  MODIFY `imageID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `Users`
--
ALTER TABLE `Users`
  MODIFY `userID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `Builds`
--
ALTER TABLE `Builds`
  ADD CONSTRAINT `Builds_ibfk_1` FOREIGN KEY (`userID`) REFERENCES `Users` (`userID`),
  ADD CONSTRAINT `Builds_ibfk_2` FOREIGN KEY (`imageID`) REFERENCES `Images` (`imageID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
