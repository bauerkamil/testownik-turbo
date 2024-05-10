import Client from "@/api/Client";
import Navbar from "@/components/navbar/Navbar";
import TestCard from "@/components/test-card/TestCard";
import { Label } from "@/components/ui";
import { Input } from "@/components/ui/input";
import { NavbarPages } from "@/shared/enums/NavbarPages";
import { ITest } from "@/shared/interfaces";
import { useState, useEffect } from "react";

const SearchCourse: React.FC = () => {
  const [tests, setTests] = useState<ITest[]>([]);
  const [filteredTests, setFilteredTests] = useState<ITest[]>([]);
  const [nameFilter, setNameFilter] = useState<string>("");
  const [teacherFilter, setTeacherFilter] = useState<string>("");
  const [courseFilter, setCourseFilter] = useState<string>("");

  const handleDeleted = (id: string) => {
    setTests(tests.filter((test) => test.id !== id));
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const testsData = await Client.getTests();
        console.log("Tests data:", testsData);

        setTests(testsData);
      } catch (error) {
        console.error("An error occurred while fetching tests:", error);
      }
    };

    fetchData();
  }, []);

  useEffect(() => {
    const splitTeacher = teacherFilter.split(" ");
    let tests1 = tests;
    if (splitTeacher.length > 0) {
      tests1 = tests1.filter(
        (t) =>
          t.course?.teacher?.name
            .toLowerCase()
            .includes(splitTeacher[0].toLowerCase()) ||
          t.course?.teacher?.surname
            .toLowerCase()
            .includes(splitTeacher[0].toLowerCase())
      );
    }
    if (splitTeacher.length > 1) {
      tests1 = tests1.filter(
        (t) =>
          t.course?.teacher?.name
            .toLowerCase()
            .includes(splitTeacher[1].toLowerCase()) ||
          t.course?.teacher?.surname
            .toLowerCase()
            .includes(splitTeacher[1].toLowerCase())
      );
    }
    if (nameFilter) {
      tests1 = tests1.filter((t) =>
        t.name?.toLowerCase().includes(nameFilter.toLowerCase())
      );
    }
    if (courseFilter) {
      tests1 = tests1.filter((t) =>
        t.course?.name?.toLowerCase().includes(courseFilter.toLowerCase())
      );
    }
    setFilteredTests(tests1);
  }, [courseFilter, nameFilter, teacherFilter, tests]);

  return (
    <div className="flex min-h-screen w-full flex-col">
      <Navbar page={NavbarPages.SearchCourse} />
      <main className="flex flex-1 flex-col gap-4 p-4 md:gap-8 md:p-8">
        <div className="grid gap-2 text-center">
          <div className="text-4xl font-bold">
            WYSZUKAJ SWÓJ ZBAWCZY TESTOWNIK
          </div>
          <p className="text-balance text-muted-foreground">
            I skończ wreszcie udawać że się uczysz
          </p>
        </div>
        <div className="flex flex-row gap-4">
          <div className="grow">
            <Label>Nazwa testo</Label>
            <Input
              value={nameFilter}
              onChange={(e) => setNameFilter(e.target.value)}
              type="search"
              placeholder="Wyszukaj..."
            />
          </div>
          <div className="grow">
            <Label>Prowadzący</Label>
            <Input
              value={teacherFilter}
              onChange={(e) => setTeacherFilter(e.target.value)}
              type="search"
              placeholder="Wyszukaj..."
            />
          </div>
          <div className="grow">
            <Label>Nazwa przedmiotu</Label>
            <Input
              value={courseFilter}
              onChange={(e) => setCourseFilter(e.target.value)}
              type="search"
              placeholder="Wyszukaj..."
            />
          </div>
        </div>
        <div className="text-2xl">Testowniki:</div>
        <div className="grid gap-4 md:grid-cols-2">
          {filteredTests.map((test) => (
            <TestCard key={test.id} test={test} onDeleted={handleDeleted} />
          ))}
        </div>
      </main>
    </div>
  );
};

export default SearchCourse;
