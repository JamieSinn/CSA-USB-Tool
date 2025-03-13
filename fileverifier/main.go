package main

import (
	"crypto/md5"
	"encoding/json"
	"flag"
	"fmt"
	"io"
	"net/http"
	"os"
)

var completed = make(chan Software, 100)
var failed = make(chan Software, 100)
var filePath string

type ProgramSeason struct {
	Year     int        `json:"Year,omitempty"`
	Program  string     `json:"Program,omitempty"`
	Software []Software `json:"Software,omitempty"`
}
type Software struct {
	Name        string   `json:"Name,omitempty"`
	Description string   `json:"Description,omitempty"`
	Tags        []string `json:"Tags,omitempty"`
	FileName    any      `json:"FileName,omitempty"`
	URI         string   `json:"Uri,omitempty"`      // String
	Hash        any      `json:"Hash,omitempty"`     // String or null
	Platform    any      `json:"Platform,omitempty"` // String or null
}

func (s *Software) Download() error {
	if s.URI == "" {
		return fmt.Errorf("URI is empty")
	}
	if s.Hash == nil {
		fmt.Println("Skipping", s.Name, "as hash is not provided")
		return nil
	}
	fmt.Println("Downloading", s.Name)
	client := http.DefaultClient
	resp, err := client.Get(s.URI)
	if err != nil {
		return err
	}
	defer resp.Body.Close()
	hasher := md5.New()
	defer hasher.Reset()
	_, err = io.Copy(hasher, resp.Body)
	// Calculate the hash
	hash := fmt.Sprintf("%x", hasher.Sum(nil))
	if s.Hash != hash {
		return fmt.Errorf("ERROR - %s: hash mismatch - expected: %s != %s", s.URI, s.Hash, hash)
	}
	return nil
}

func main() {

	flag.StringVar(&filePath, "file", "", "File path")
	flag.Parse()

	if filePath == "" {
		fmt.Println("Please provide a file path")
		flag.PrintDefaults()
		os.Exit(1)
	}

	_, err := os.Stat(filePath)
	if os.IsNotExist(err) {
		fmt.Println("File does not exist")
		flag.PrintDefaults()
		os.Exit(1)
	}
	if err = checkFile(filePath); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}

func checkFile(filePath string) (err error) {
	file, err := os.Open(filePath)
	if err != nil {
		fmt.Println(err)
		return err
	}
	err = file.Close()
	if err != nil {
		return err
	}
	rawBytes, err := os.ReadFile(filePath)

	if err != nil {
		fmt.Println(err)
		return err
	}
	// Decode the JSON
	var programSeason ProgramSeason
	err = json.Unmarshal(rawBytes, &programSeason)
	if err != nil {
		fmt.Println(err)
		return err
	}

	// Download the software
	for _, s := range programSeason.Software {
		software := s
		go func() {
			e2 := software.Download()
			if e2 != nil {
				fmt.Println(e2)
				failed <- software
			} else {
				completed <- software
			}
		}()
	}

	completedCount := 0
	failedCount := 0
	for {
		select {
		case software := <-completed:
			fmt.Printf("Downloaded %s successfully (%d/%d)\n", software.Name, completedCount+failedCount, len(programSeason.Software))
			completedCount++
		}
		select {
		case software := <-failed:
			fmt.Printf("Failed to download %s (%d/%d)\n", software.Name, failedCount+completedCount, len(programSeason.Software))
			failedCount++
		}
		if completedCount+failedCount >= len(programSeason.Software) {
			fmt.Println("All downloads finished")
			break
		}
	}
	return nil
}
